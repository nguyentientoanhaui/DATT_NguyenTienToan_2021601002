import os
import json
import re
from datetime import datetime
from typing import Dict, List, Any, Optional, Tuple

import pyodbc


def _parse_dotnet_connection_string(conn_str: str) -> Dict[str, str]:
    """
    Parse a typical .NET style SQL Server connection string into a dict.
    Example: "Server=.;Database=Db;Trusted_Connection=True;TrustServerCertificate=True;"
    """
    parts = [p.strip() for p in conn_str.split(";") if p.strip()]
    kv: Dict[str, str] = {}
    for part in parts:
        if "=" in part:
            k, v = part.split("=", 1)
            kv[k.strip().lower()] = v.strip()
    return kv


def build_pyodbc_connection_string() -> Tuple[str, str, str]:
    """
    Build a pyodbc connection string from appsettings.json if available.
    Returns a tuple: (pyodbc_conn_str, server, database)
    Fallback to reasonable defaults if not found.
    """
    appsettings_paths = [
        os.path.join(os.getcwd(), "appsettings.Development.json"),
        os.path.join(os.getcwd(), "appsettings.json"),
    ]

    connected_db_raw: Optional[str] = None
    for path in appsettings_paths:
        if os.path.exists(path):
            try:
                with open(path, "r", encoding="utf-8") as f:
                    data = json.load(f)
                conn_section = data.get("ConnectionStrings", {})
                connected_db_raw = conn_section.get("ConnectedDb")
                if connected_db_raw:
                    break
            except Exception:
                pass

    server = ".\\SQLEXPRESS"
    database = "Shopping_Demo"
    trusted = True
    trust_cert = True
    user_id: Optional[str] = None
    password: Optional[str] = None

    if connected_db_raw:
        kv = _parse_dotnet_connection_string(connected_db_raw)
        server = kv.get("server", kv.get("data source", server))
        database = kv.get("database", kv.get("initial catalog", database))
        trusted_val = kv.get("trusted_connection", kv.get("integrated security"))
        trusted = (str(trusted_val).lower() in {"true", "yes", "sspi"}) if trusted_val is not None else trusted
        trust_cert_val = kv.get("trustservercertificate")
        if trust_cert_val is not None:
            trust_cert = str(trust_cert_val).lower() in {"true", "yes"}
        user_id = kv.get("user id", kv.get("uid"))
        password = kv.get("password", kv.get("pwd"))

    # Normalize server (remove tcp: prefix if present)
    server = re.sub(r"^tcp:", "", server, flags=re.IGNORECASE)

    # Build pyodbc string
    parts = ["Driver={ODBC Driver 17 for SQL Server}", f"Server={server}", f"Database={database}"]
    if user_id and password and not trusted:
        parts.append(f"Uid={user_id}")
        parts.append(f"Pwd={password}")
    else:
        parts.append("Trusted_Connection=yes" if trusted else "Trusted_Connection=no")
    if trust_cert:
        parts.append("TrustServerCertificate=yes")

    return ";".join(parts) + ";", server, database


class DatabaseIntrospector:
    def __init__(self, connection_string: str, database_name_hint: str = "") -> None:
        self.connection_string = connection_string
        self.database_name_hint = database_name_hint
        self.connection: Optional[pyodbc.Connection] = None

    # ----------------------- Connection -----------------------
    def connect(self) -> bool:
        try:
            self.connection = pyodbc.connect(self.connection_string)
            print("✅ Kết nối database thành công!")
            return True
        except Exception as exc:
            print(f"❌ Lỗi kết nối database: {exc}")
            return False

    def close(self) -> None:
        if self.connection:
            self.connection.close()
            print("🔒 Đã đóng kết nối database")

    # ----------------------- Helpers -----------------------
    def _fetchall(self, query: str, params: Optional[List[Any]] = None) -> List[tuple]:
        assert self.connection is not None
        cursor = self.connection.cursor()
        if params:
            cursor.execute(query, params)
        else:
            cursor.execute(query)
        rows = cursor.fetchall()
        return rows

    def _fetchone(self, query: str) -> Optional[tuple]:
        assert self.connection is not None
        cursor = self.connection.cursor()
        cursor.execute(query)
        return cursor.fetchone()

    # ----------------------- Introspection queries -----------------------
    def get_all_tables(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT TABLE_SCHEMA, TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE = 'BASE TABLE'
            ORDER BY TABLE_SCHEMA, TABLE_NAME
            """
        )
        try:
            rows = self._fetchall(query)
            tables = [
                {"schema": r[0], "name": r[1], "full_name": f"{r[0]}.{r[1]}"} for r in rows
            ]
            return tables
        except Exception as exc:
            print(f"❌ Lỗi lấy danh sách bảng: {exc}")
            return []

    def get_columns_for_table(self, schema: str, table: str) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT 
                COLUMN_NAME,
                DATA_TYPE,
                IS_NULLABLE,
                COLUMN_DEFAULT,
                CHARACTER_MAXIMUM_LENGTH,
                NUMERIC_PRECISION,
                NUMERIC_SCALE
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = ? AND TABLE_NAME = ?
            ORDER BY ORDINAL_POSITION
            """
        )
        try:
            rows = self._fetchall(query, [schema, table])
            return [
                {
                    "column_name": r[0],
                    "data_type": r[1],
                    "is_nullable": (r[2] == "YES"),
                    "column_default": r[3],
                    "character_max_length": r[4],
                    "numeric_precision": r[5],
                    "numeric_scale": r[6],
                }
                for r in rows
            ]
        except Exception as exc:
            print(f"❌ Lỗi lấy cột cho bảng {schema}.{table}: {exc}")
            return []

    def get_primary_keys(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT KU.TABLE_SCHEMA, KU.TABLE_NAME, KU.COLUMN_NAME, KU.ORDINAL_POSITION, TC.CONSTRAINT_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS TC
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KU
              ON TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME
            WHERE TC.CONSTRAINT_TYPE = 'PRIMARY KEY'
            ORDER BY KU.TABLE_SCHEMA, KU.TABLE_NAME, KU.ORDINAL_POSITION
            """
        )
        try:
            rows = self._fetchall(query)
            return [
                {
                    "schema": r[0],
                    "table": r[1],
                    "column": r[2],
                    "key_ordinal": r[3],
                    "constraint_name": r[4],
                }
                for r in rows
            ]
        except Exception as exc:
            print(f"❌ Lỗi lấy PRIMARY KEY: {exc}")
            return []

    def get_foreign_keys(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT 
                fk.TABLE_SCHEMA as FK_SCHEMA,
                fk.TABLE_NAME as FK_TABLE,
                fk.COLUMN_NAME as FK_COLUMN,
                pk.TABLE_SCHEMA as PK_SCHEMA,
                pk.TABLE_NAME as PK_TABLE,
                pk.COLUMN_NAME as PK_COLUMN,
                fk.CONSTRAINT_NAME
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk
            INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc 
                ON fk.CONSTRAINT_NAME = rc.CONSTRAINT_NAME
            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk 
                ON rc.UNIQUE_CONSTRAINT_NAME = pk.CONSTRAINT_NAME
            WHERE fk.CONSTRAINT_NAME LIKE 'FK_%'
            ORDER BY fk.TABLE_SCHEMA, fk.TABLE_NAME, fk.COLUMN_NAME
            """
        )
        try:
            rows = self._fetchall(query)
            return [
                {
                    "fk_schema": r[0],
                    "fk_table": r[1],
                    "fk_column": r[2],
                    "pk_schema": r[3],
                    "pk_table": r[4],
                    "pk_column": r[5],
                    "constraint_name": r[6],
                }
                for r in rows
            ]
        except Exception as exc:
            print(f"❌ Lỗi lấy FOREIGN KEY: {exc}")
            return []

    def get_unique_constraints(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT TC.TABLE_SCHEMA, TC.TABLE_NAME, KU.COLUMN_NAME, TC.CONSTRAINT_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KU
              ON TC.CONSTRAINT_NAME = KU.CONSTRAINT_NAME
            WHERE TC.CONSTRAINT_TYPE = 'UNIQUE'
            ORDER BY TC.TABLE_SCHEMA, TC.TABLE_NAME
            """
        )
        try:
            rows = self._fetchall(query)
            return [
                {
                    "schema": r[0],
                    "table": r[1],
                    "column": r[2],
                    "constraint_name": r[3],
                }
                for r in rows
            ]
        except Exception as exc:
            print(f"❌ Lỗi lấy UNIQUE CONSTRAINTS: {exc}")
            return []

    def get_check_constraints(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT CC.CONSTRAINT_NAME, CTU.TABLE_SCHEMA, CTU.TABLE_NAME, CC.CHECK_CLAUSE
            FROM INFORMATION_SCHEMA.CHECK_CONSTRAINTS CC
            JOIN INFORMATION_SCHEMA.CONSTRAINT_TABLE_USAGE CTU
              ON CC.CONSTRAINT_NAME = CTU.CONSTRAINT_NAME
            ORDER BY CTU.TABLE_SCHEMA, CTU.TABLE_NAME
            """
        )
        try:
            rows = self._fetchall(query)
            return [
                {
                    "constraint_name": r[0],
                    "schema": r[1],
                    "table": r[2],
                    "check_clause": r[3],
                }
                for r in rows
            ]
        except Exception as exc:
            print(f"❌ Lỗi lấy CHECK CONSTRAINTS: {exc}")
            return []

    def get_indexes(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT 
                s.name as schema_name,
                t.name as table_name,
                i.name as index_name,
                i.is_unique,
                i.is_primary_key,
                c.name as column_name,
                ic.key_ordinal,
                ic.is_included_column
            FROM sys.indexes i
            JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            JOIN sys.tables t ON i.object_id = t.object_id
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            WHERE i.is_hypothetical = 0 AND t.is_ms_shipped = 0
            ORDER BY s.name, t.name, i.name, ic.key_ordinal
            """
        )
        try:
            rows = self._fetchall(query)
            return [
                {
                    "schema": r[0],
                    "table": r[1],
                    "index_name": r[2],
                    "is_unique": bool(r[3]),
                    "is_primary_key": bool(r[4]),
                    "column": r[5],
                    "key_ordinal": r[6],
                    "is_included_column": bool(r[7]),
                }
                for r in rows
            ]
        except Exception as exc:
            print(f"❌ Lỗi lấy INDEXES: {exc}")
            return []

    def get_views(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT TABLE_SCHEMA, TABLE_NAME
            FROM INFORMATION_SCHEMA.VIEWS
            ORDER BY TABLE_SCHEMA, TABLE_NAME
            """
        )
        try:
            rows = self._fetchall(query)
            return [{"schema": r[0], "name": r[1], "full_name": f"{r[0]}.{r[1]}"} for r in rows]
        except Exception as exc:
            print(f"❌ Lỗi lấy VIEWS: {exc}")
            return []

    def get_stored_procedures(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT s.name as schema_name, p.name as proc_name
            FROM sys.procedures p
            JOIN sys.schemas s ON p.schema_id = s.schema_id
            ORDER BY s.name, p.name
            """
        )
        try:
            rows = self._fetchall(query)
            return [{"schema": r[0], "name": r[1], "full_name": f"{r[0]}.{r[1]}"} for r in rows]
        except Exception as exc:
            print(f"❌ Lỗi lấy STORED PROCEDURES: {exc}")
            return []

    def get_scalar_table_functions(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT s.name as schema_name, o.name as obj_name, o.type
            FROM sys.objects o
            JOIN sys.schemas s ON o.schema_id = s.schema_id
            WHERE o.type IN ('FN','IF','TF')
            ORDER BY s.name, o.name
            """
        )
        try:
            rows = self._fetchall(query)
            return [
                {"schema": r[0], "name": r[1], "type": r[2], "full_name": f"{r[0]}.{r[1]}"}
                for r in rows
            ]
        except Exception as exc:
            print(f"❌ Lỗi lấy FUNCTIONS: {exc}")
            return []

    def count_rows(self, schema: str, table: str) -> int:
        try:
            row = self._fetchone(f"SELECT COUNT(*) FROM [{schema}].[{table}]")
            return int(row[0]) if row else 0
        except Exception as exc:
            print(f"❌ Lỗi đếm dòng {schema}.{table}: {exc}")
            return 0

    def sample_rows(self, schema: str, table: str, limit: int = 5) -> List[Dict[str, Any]]:
        try:
            assert self.connection is not None
            cursor = self.connection.cursor()
            cursor.execute(f"SELECT TOP {limit} * FROM [{schema}].[{table}]")
            columns = [c[0] for c in cursor.description]
            rows = cursor.fetchall()
            out: List[Dict[str, Any]] = []
            for r in rows:
                out.append({columns[i]: r[i] for i in range(len(columns))})
            return out
        except Exception as exc:
            print(f"❌ Lỗi lấy dữ liệu mẫu {schema}.{table}: {exc}")
            return []

    # ----------------------- Orchestration -----------------------
    def introspect(self) -> Dict[str, Any]:
        print("🔍 Bắt đầu đọc hiểu toàn bộ database...")
        result: Dict[str, Any] = {
            "generated_at": datetime.now().isoformat(),
            "database_name": self.database_name_hint or "",
            "tables": {},
            "relationships": {
                "primary_keys": [],
                "foreign_keys": [],
                "unique_constraints": [],
                "check_constraints": [],
                "indexes": [],
            },
            "views": [],
            "stored_procedures": [],
            "functions": [],
        }

        # Catalog objects
        tables = self.get_all_tables()
        pks = self.get_primary_keys()
        fks = self.get_foreign_keys()
        uniques = self.get_unique_constraints()
        checks = self.get_check_constraints()
        indexes = self.get_indexes()
        views = self.get_views()
        sprocs = self.get_stored_procedures()
        funcs = self.get_scalar_table_functions()

        result["relationships"]["primary_keys"] = pks
        result["relationships"]["foreign_keys"] = fks
        result["relationships"]["unique_constraints"] = uniques
        result["relationships"]["check_constraints"] = checks
        result["relationships"]["indexes"] = indexes
        result["views"] = views
        result["stored_procedures"] = sprocs
        result["functions"] = funcs

        # Per table details
        for t in tables:
            schema = t["schema"]
            name = t["name"]
            full = t["full_name"]
            print(f"📋 Bảng: {full}")
            cols = self.get_columns_for_table(schema, name)
            row_count = self.count_rows(schema, name)
            sample = self.sample_rows(schema, name, 5)
            result["tables"][full] = {
                "schema": schema,
                "name": name,
                "columns": cols,
                "row_count": row_count,
                "sample_rows": sample,
            }
            print(f"   📊 Số dòng: {row_count}")
            print(f"   📋 Số cột: {len(cols)}")

        return result


def save_report(report: Dict[str, Any], filename: str = "db_introspection_report.json") -> None:
    with open(filename, "w", encoding="utf-8") as f:
        json.dump(report, f, ensure_ascii=False, indent=2, default=str)
    print(f"📄 Đã lưu báo cáo JSON: {filename}")


def main() -> None:
    conn_str, server, db = build_pyodbc_connection_string()
    print("🔧 Connection (server -> database):", server, "->", db)
    inspector = DatabaseIntrospector(conn_str, database_name_hint=db)
    if not inspector.connect():
        return
    try:
        report = inspector.introspect()
        save_report(report)
        print("\n✅ Hoàn thành đọc hiểu database!")
    finally:
        inspector.close()


if __name__ == "__main__":
    main()


