import os
import json
from datetime import datetime
from typing import Dict, List, Any, Optional, Tuple

import pyodbc


def _parse_dotnet_connection_string(conn_str: str) -> Dict[str, str]:
    parts = [p.strip() for p in conn_str.split(";") if p.strip()]
    kv: Dict[str, str] = {}
    for part in parts:
        if "=" in part:
            k, v = part.split("=", 1)
            kv[k.strip().lower()] = v.strip()
    return kv


def build_pyodbc_connection_string() -> Tuple[str, str, str]:
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
        if trusted_val is not None:
            trusted = str(trusted_val).lower() in {"true", "yes", "sspi"}
        trust_cert_val = kv.get("trustservercertificate")
        if trust_cert_val is not None:
            trust_cert = str(trust_cert_val).lower() in {"true", "yes"}
        user_id = kv.get("user id", kv.get("uid"))
        password = kv.get("password", kv.get("pwd"))

    parts = ["Driver={ODBC Driver 17 for SQL Server}", f"Server={server}", f"Database={database}"]
    if user_id and password and not trusted:
        parts.append(f"Uid={user_id}")
        parts.append(f"Pwd={password}")
    else:
        parts.append("Trusted_Connection=yes" if trusted else "Trusted_Connection=no")
    if trust_cert:
        parts.append("TrustServerCertificate=yes")

    return ";".join(parts) + ";", server, database


NUMERIC_TYPES = {
    "bigint",
    "int",
    "smallint",
    "tinyint",
    "bit",
    "decimal",
    "numeric",
    "float",
    "real",
    "money",
    "smallmoney",
}

DATETIME_TYPES = {"datetime", "datetime2", "smalldatetime", "date", "time", "datetimeoffset"}

STRING_TYPES = {"varchar", "nvarchar", "char", "nchar", "text", "ntext"}

BINARY_TYPES = {"varbinary", "binary", "image"}


class DeepDatabaseProfiler:
    def __init__(self, connection_string: str, database_name: str) -> None:
        self.connection_string = connection_string
        self.database_name = database_name
        self.connection: Optional[pyodbc.Connection] = None

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

    def _fetchall(self, query: str, params: Optional[List[Any]] = None) -> List[tuple]:
        assert self.connection is not None
        cursor = self.connection.cursor()
        if params:
            cursor.execute(query, params)
        else:
            cursor.execute(query)
        return cursor.fetchall()

    def _fetchone(self, query: str, params: Optional[List[Any]] = None) -> Optional[tuple]:
        assert self.connection is not None
        cursor = self.connection.cursor()
        if params:
            cursor.execute(query, params)
        else:
            cursor.execute(query)
        return cursor.fetchone()

    # ---------------- Schema listing ----------------
    def get_tables(self) -> List[Dict[str, str]]:
        query = (
            """
            SELECT TABLE_SCHEMA, TABLE_NAME
            FROM INFORMATION_SCHEMA.TABLES
            WHERE TABLE_TYPE='BASE TABLE'
            ORDER BY TABLE_SCHEMA, TABLE_NAME
            """
        )
        rows = self._fetchall(query)
        return [{"schema": r[0], "name": r[1]} for r in rows]

    def get_columns_detail(self, schema: str, table: str) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT 
                c.column_id,
                c.name AS column_name,
                ty.name AS data_type,
                c.max_length,
                c.precision,
                c.scale,
                c.is_nullable,
                c.is_identity,
                c.is_computed,
                c.collation_name,
                dc.definition AS default_definition
            FROM sys.columns c
            JOIN sys.tables t ON c.object_id = t.object_id
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            JOIN sys.types ty ON c.user_type_id = ty.user_type_id
            LEFT JOIN sys.default_constraints dc ON c.default_object_id = dc.object_id
            WHERE s.name = ? AND t.name = ?
            ORDER BY c.column_id
            """
        )
        rows = self._fetchall(query, [schema, table])
        cols: List[Dict[str, Any]] = []
        for r in rows:
            cols.append(
                {
                    "ordinal": int(r[0]),
                    "column_name": r[1],
                    "data_type": r[2],
                    "max_length": r[3],
                    "precision": r[4],
                    "scale": r[5],
                    "is_nullable": bool(r[6]),
                    "is_identity": bool(r[7]),
                    "is_computed": bool(r[8]),
                    "collation": r[9],
                    "default": r[10],
                }
            )
        return cols

    def get_primary_keys(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT s.name, t.name, c.name, k.key_ordinal, kc.name
            FROM sys.key_constraints kc
            JOIN sys.tables t ON kc.parent_object_id = t.object_id
            JOIN sys.schemas s ON t.schema_id = s.schema_id
            JOIN sys.index_columns k ON kc.unique_index_id = k.index_id AND kc.parent_object_id = k.object_id
            JOIN sys.columns c ON k.object_id = c.object_id AND k.column_id = c.column_id
            WHERE kc.type = 'PK'
            ORDER BY s.name, t.name, k.key_ordinal
            """
        )
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

    def get_foreign_keys_extended(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT 
                sch_fk.name AS fk_schema,
                t_fk.name AS fk_table,
                c_fk.name AS fk_column,
                sch_pk.name AS pk_schema,
                t_pk.name AS pk_table,
                c_pk.name AS pk_column,
                fk.name AS constraint_name,
                fk.delete_referential_action_desc,
                fk.update_referential_action_desc
            FROM sys.foreign_key_columns fkc
            JOIN sys.foreign_keys fk ON fkc.constraint_object_id = fk.object_id
            JOIN sys.tables t_fk ON fkc.parent_object_id = t_fk.object_id
            JOIN sys.schemas sch_fk ON t_fk.schema_id = sch_fk.schema_id
            JOIN sys.columns c_fk ON fkc.parent_object_id = c_fk.object_id AND fkc.parent_column_id = c_fk.column_id
            JOIN sys.tables t_pk ON fkc.referenced_object_id = t_pk.object_id
            JOIN sys.schemas sch_pk ON t_pk.schema_id = sch_pk.schema_id
            JOIN sys.columns c_pk ON fkc.referenced_object_id = c_pk.object_id AND fkc.referenced_column_id = c_pk.column_id
            ORDER BY sch_fk.name, t_fk.name, c_fk.name
            """
        )
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
                "on_delete": r[7],
                "on_update": r[8],
            }
            for r in rows
        ]

    def get_indexes_grouped(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT 
                sch.name AS schema_name,
                t.name AS table_name,
                i.name AS index_name,
                i.is_unique,
                i.is_primary_key,
                c.name AS column_name,
                ic.key_ordinal,
                ic.is_included_column
            FROM sys.indexes i
            JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
            JOIN sys.columns c ON ic.object_id = c.object_id AND ic.column_id = c.column_id
            JOIN sys.tables t ON i.object_id = t.object_id
            JOIN sys.schemas sch ON t.schema_id = sch.schema_id
            WHERE i.is_hypothetical = 0 AND t.is_ms_shipped = 0
            ORDER BY sch.name, t.name, i.name, ic.key_ordinal, ic.is_included_column
            """
        )
        rows = self._fetchall(query)
        grouped: Dict[Tuple[str, str, str], Dict[str, Any]] = {}
        for r in rows:
            key = (r[0], r[1], r[2])
            entry = grouped.setdefault(
                key,
                {
                    "schema": r[0],
                    "table": r[1],
                    "index_name": r[2],
                    "is_unique": bool(r[3]),
                    "is_primary_key": bool(r[4]),
                    "key_columns": [],
                    "included_columns": [],
                },
            )
            col_name = r[5]
            if bool(r[7]):
                entry["included_columns"].append(col_name)
            else:
                entry["key_columns"].append({"column": col_name, "order": r[6]})
        return list(grouped.values())

    def get_triggers(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT sch.name AS schema_name, t.name AS table_name, tr.name AS trigger_name,
                   tr.is_disabled,
                   te.type_desc AS event_type,
                   OBJECTPROPERTY(tr.object_id,'ExecIsTriggerDisabled') AS is_disabled2
            FROM sys.triggers tr
            JOIN sys.tables t ON tr.parent_id = t.object_id
            JOIN sys.schemas sch ON t.schema_id = sch.schema_id
            LEFT JOIN sys.trigger_events te ON te.object_id = tr.object_id
            ORDER BY sch.name, t.name, tr.name
            """
        )
        rows = self._fetchall(query)
        grouped: Dict[Tuple[str, str, str], Dict[str, Any]] = {}
        for r in rows:
            key = (r[0], r[1], r[2])
            entry = grouped.setdefault(
                key,
                {
                    "schema": r[0],
                    "table": r[1],
                    "trigger_name": r[2],
                    "is_disabled": bool(r[3] or r[5]),
                    "events": [],
                },
            )
            if r[4] and r[4] not in entry["events"]:
                entry["events"].append(r[4])
        return list(grouped.values())

    def get_table_sizes(self) -> List[Dict[str, Any]]:
        query = (
            """
            SELECT sch.name AS schema_name,
                   t.name AS table_name,
                   SUM(ps.row_count) AS row_count,
                   SUM(ps.reserved_page_count)*8 AS reserved_kb,
                   SUM(ps.used_page_count)*8 AS used_kb
            FROM sys.dm_db_partition_stats ps
            JOIN sys.tables t ON ps.object_id = t.object_id
            JOIN sys.schemas sch ON t.schema_id = sch.schema_id
            GROUP BY sch.name, t.name
            ORDER BY sch.name, t.name
            """
        )
        try:
            rows = self._fetchall(query)
            return [
                {
                    "schema": r[0],
                    "table": r[1],
                    "row_count": int(r[2]),
                    "reserved_kb": int(r[3]),
                    "used_kb": int(r[4]),
                }
                for r in rows
            ]
        except Exception:
            return []

    # ---------------- Data profiling ----------------
    def _scalar(self, query: str) -> Optional[Any]:
        try:
            row = self._fetchone(query)
            return row[0] if row else None
        except Exception:
            return None

    def profile_column(self, schema: str, table: str, column: Dict[str, Any]) -> Dict[str, Any]:
        col = column["column_name"]
        dtype = (column["data_type"] or "").lower()
        fq = f"[{schema}].[{table}]"
        q_nulls = f"SELECT SUM(CASE WHEN [{col}] IS NULL THEN 1 ELSE 0 END) FROM {fq}"
        q_non_nulls = f"SELECT SUM(CASE WHEN [{col}] IS NULL THEN 0 ELSE 1 END) FROM {fq}"
        nulls = self._scalar(q_nulls) or 0
        non_nulls = self._scalar(q_non_nulls) or 0

        profile: Dict[str, Any] = {
            "column": col,
            "data_type": dtype,
            "null_count": int(nulls),
            "non_null_count": int(non_nulls),
            "distinct_count": None,
            "stats": {},
            "top_values": [],
        }

        # Distinct count (skip for binary)
        if dtype not in BINARY_TYPES:
            distinct = self._scalar(f"SELECT COUNT(DISTINCT [{col}]) FROM {fq} WHERE [{col}] IS NOT NULL")
            profile["distinct_count"] = int(distinct) if distinct is not None else None

        # Type-specific stats
        if dtype in NUMERIC_TYPES:
            min_v = self._scalar(f"SELECT MIN([{col}]) FROM {fq}")
            max_v = self._scalar(f"SELECT MAX([{col}]) FROM {fq}")
            avg_v = self._scalar(f"SELECT AVG(CAST([{col}] AS FLOAT)) FROM {fq}")
            profile["stats"] = {"min": min_v, "max": max_v, "avg": avg_v}
        elif dtype in DATETIME_TYPES:
            min_v = self._scalar(f"SELECT MIN([{col}]) FROM {fq}")
            max_v = self._scalar(f"SELECT MAX([{col}]) FROM {fq}")
            # Avoid fetching raw datetimeoffset values that pyodbc may not support by converting to NVARCHAR
            profile["stats"] = {
                "min": str(min_v) if min_v is not None else None,
                "max": str(max_v) if max_v is not None else None,
            }
        elif dtype in STRING_TYPES:
            avg_len = self._scalar(f"SELECT AVG(CAST(LEN([{col}]) AS FLOAT)) FROM {fq} WHERE [{col}] IS NOT NULL")
            max_len = self._scalar(f"SELECT MAX(LEN([{col}])) FROM {fq}")
            profile["stats"] = {"avg_length": avg_len, "max_length": max_len}

        # Top values (skip for binary and wide text types that may be huge)
        if dtype not in BINARY_TYPES:
            try:
                query = (
                    f"SELECT TOP 5 CAST([{col}] AS NVARCHAR(200)) AS v, COUNT(*) AS c "
                    f"FROM {fq} WHERE [{col}] IS NOT NULL GROUP BY [{col}] ORDER BY c DESC"
                )
                rows = self._fetchall(query)
                profile["top_values"] = [{"value": r[0], "count": int(r[1])} for r in rows]
            except Exception:
                profile["top_values"] = []

        return profile

    def profile_table(self, schema: str, table: str) -> Dict[str, Any]:
        columns = self.get_columns_detail(schema, table)
        col_profiles: List[Dict[str, Any]] = []
        for col in columns:
            col_profiles.append(self.profile_column(schema, table, col))
        return {
            "columns": columns,
            "column_profiles": col_profiles,
        }

    # ---------------- Orchestration ----------------
    def run(self) -> Dict[str, Any]:
        print("🔍 Bắt đầu phân tích chuyên sâu database...")
        report: Dict[str, Any] = {
            "generated_at": datetime.now().isoformat(),
            "database_name": self.database_name,
            "tables": {},
            "constraints": {
                "primary_keys": self.get_primary_keys(),
                "foreign_keys": self.get_foreign_keys_extended(),
            },
            "indexes": self.get_indexes_grouped(),
            "triggers": self.get_triggers(),
            "sizes": self.get_table_sizes(),
        }

        tables = self.get_tables()
        for t in tables:
            schema = t["schema"]
            name = t["name"]
            print(f"📋 Đang phân tích bảng: {schema}.{name}")
            report["tables"][f"{schema}.{name}"] = self.profile_table(schema, name)
        return report


def save_reports(report: Dict[str, Any]) -> None:
    json_path = "deep_db_profile.json"
    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(report, f, ensure_ascii=False, indent=2, default=str)
    print(f"📄 Đã lưu báo cáo JSON: {json_path}")


def main() -> None:
    conn_str, server, db = build_pyodbc_connection_string()
    print("🔧 Connection (server -> database):", server, "->", db)
    profiler = DeepDatabaseProfiler(conn_str, db)
    if not profiler.connect():
        return
    try:
        report = profiler.run()
        save_reports(report)
        print("\n✅ Hoàn thành phân tích chuyên sâu!")
    finally:
        profiler.close()


if __name__ == "__main__":
    main()


