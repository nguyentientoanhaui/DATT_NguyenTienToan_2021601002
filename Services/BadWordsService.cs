using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Shopping_Demo.Services
{
    public interface IBadWordsService
    {
        bool ContainsBadWords(string text);
        string CleanText(string text);
        List<string> FindBadWords(string text);
    }

    public class BadWordsService : IBadWordsService
    {
        private readonly List<string> _badWords;

        public BadWordsService()
        {
            // Danh sách từ khóa nhạy cảm - cần mở rộng theo nhu cầu
            _badWords = new List<string>
            {
                "admin",
                "hack",
                "crack",
                "cheat",
                "scam",
                "lừa đảo",
                "lừa đão",
                "virus",
                "malware",
                "spam"
                // Thêm các từ khóa nhạy cảm khác
            };
        }

        public bool ContainsBadWords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            text = text.ToLower();

            // Kiểm tra từng từ trong danh sách từ khóa nhạy cảm
            foreach (var word in _badWords)
            {
                // Sử dụng biểu thức chính quy để tìm từ trong chuỗi
                // \b đảm bảo tìm từ hoàn chỉnh, không phải một phần của từ khác
                if (Regex.IsMatch(text, $@"\b{word}\b", RegexOptions.IgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public List<string> FindBadWords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            text = text.ToLower();
            List<string> foundWords = new List<string>();

            foreach (var word in _badWords)
            {
                if (Regex.IsMatch(text, $@"\b{word}\b", RegexOptions.IgnoreCase))
                {
                    foundWords.Add(word);
                }
            }

            return foundWords;
        }

        public string CleanText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            foreach (var word in _badWords)
            {
                // Thay thế từ khóa nhạy cảm bằng dấu sao
                text = Regex.Replace(text, $@"\b{word}\b", new string('*', word.Length), RegexOptions.IgnoreCase);
            }

            return text;
        }
    }
}