using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace TOCSharp
{
    public static class Utils
    {
        public static string NormalizeScreenname(string screenname)
        {
            return screenname.Replace(" ", "")
                             .Replace(" ", "").ToLowerInvariant();
        }

        public static byte[] XorArray(string password, byte[] xorKey)
        {
            return XorArray(Encoding.UTF8.GetBytes(password), xorKey);
        }

        public static byte[] XorArray(byte[] password, byte[] xorKey)
        {
            byte[] result = new byte[password.Length];

            for (int i = 0; i < password.Length; i++)
            {
                byte xorByte = (byte)(password[i] ^ xorKey[i % xorKey.Length]);
                result[i] = xorByte;
            }

            return result;
        }

        /// <summary>
        /// Generate a TOC password hash
        /// </summary>
        /// <param name="username">Username, doesn't have to be normalized</param>
        /// <param name="password">Unroasted password</param>
        /// <returns>Hash</returns>
        public static int GenerateHash(string username, string password)
        {
            username = NormalizeScreenname(username); // Username should always be normalized

            int sn = username[0] - 96;
            int pw = password[0] - 96;

            int a = sn * 0x1E10 + 0xB4600;
            int b = sn * 0xB6410;
            int c = pw * a;

            return c - a + b + 0x4458600;
        }

        public static string StripHTML(string original)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(original);
            return doc.DocumentNode.InnerText;
        }

        internal static string? ExtractNextArgument(this string str, ref int startPos, char[] quoteChars)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                return null;
            }

            bool inBacktick = false;
            bool inTripleBacktick = false;
            bool inQuote = false;
            bool inEscape = false;
            List<int> removeIndices = new List<int>(str.Length - startPos);

            int i = startPos;
            for (; i < str.Length; i++)
            {
                if (!char.IsWhiteSpace(str[i]))
                {
                    break;
                }
            }

            startPos = i;

            int endPosition = -1;
            int startPosition = startPos;
            for (i = startPosition; i < str.Length; i++)
            {
                if (char.IsWhiteSpace(str[i]) && !inQuote && !inTripleBacktick && !inBacktick && !inEscape)
                {
                    endPosition = i;
                }

                if (str[i] == '\\' && str.Length > i + 1)
                {
                    if (!inEscape && !inBacktick && !inTripleBacktick)
                    {
                        inEscape = true;
                        if (str.IndexOf("\\`", i, StringComparison.Ordinal) == i || quoteChars.Any(c => str.IndexOf($"\\{c}", i, StringComparison.Ordinal) == i) || str.IndexOf("\\\\", i, StringComparison.Ordinal) == i ||
                            (str.Length >= i && char.IsWhiteSpace(str[i + 1])))
                        {
                            removeIndices.Add(i - startPosition);
                        }

                        i++;
                    }
                    else if ((inBacktick || inTripleBacktick) && str.IndexOf("\\`", i, StringComparison.Ordinal) == i)
                    {
                        inEscape = true;
                        removeIndices.Add(i - startPosition);
                        i++;
                    }
                }

                if (str[i] == '`' && !inEscape)
                {
                    bool tripleBacktick = str.IndexOf("```", i, StringComparison.Ordinal) == i;
                    if (inTripleBacktick && tripleBacktick)
                    {
                        inTripleBacktick = false;
                        i += 2;
                    }
                    else if (!inBacktick && tripleBacktick)
                    {
                        inTripleBacktick = true;
                        i += 2;
                    }

                    if (inBacktick && !tripleBacktick)
                    {
                        inBacktick = false;
                    }
                    else if (!inTripleBacktick && tripleBacktick)
                    {
                        inBacktick = true;
                    }
                }

                if (quoteChars.Contains(str[i]) && !inEscape && !inBacktick && !inTripleBacktick)
                {
                    removeIndices.Add(i - startPosition);

                    inQuote = !inQuote;
                }

                if (inEscape)
                {
                    inEscape = false;
                }

                if (endPosition != -1)
                {
                    startPos = endPosition;
                    return startPosition != endPosition
                        ? str.Substring(startPosition, endPosition - startPosition).CleanupString(removeIndices)
                        : null;
                }
            }

            startPos = str.Length;
            return startPos != startPosition ? str.Substring(startPosition).CleanupString(removeIndices) : null;
        }

        private static string CleanupString(this string s, IList<int> indices)
        {
            if (!indices.Any())
            {
                return s;
            }

            int li = indices.Last();
            int ll = 1;
            for (int x = indices.Count - 2; x >= 0; x--)
            {
                if (li - indices[x] == ll)
                {
                    ll++;
                    continue;
                }

                s = s.Remove(li - ll + 1, ll);
                li = indices[x];
                ll = 1;
            }

            return s.Remove(li - ll + 1, ll);
        }
    }
}