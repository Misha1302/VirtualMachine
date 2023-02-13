namespace Tokenizer;

using Tokenizer.Token;

public static class Extension
{
    public static bool StartsWithAny(this string s, IReadOnlyDictionary<string, TokenType> words,
        out KeyValuePair<string, TokenType> word)
    {
        IOrderedEnumerable<KeyValuePair<string, TokenType>> sortedDict =
            words.OrderBy(entry => entry.Key.Length);

        foreach (KeyValuePair<string, TokenType> item in sortedDict)
            if (s.StartsWith(item.Key))
            {
                word = item;
                return true;
            }

        word = default;
        return false;
    }
}