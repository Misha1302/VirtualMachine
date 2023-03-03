namespace Tokenizer;

using Tokenizer.Token;

public static class Extension
{
    public static bool StartsWithAny(this string s, IReadOnlyList<KeyValuePair<string, TokenType>> sortedDict,
        out KeyValuePair<string, TokenType> word)
    {
        for (int index = sortedDict.Count - 1; index >= 0; index--)
        {
            KeyValuePair<string, TokenType> item = sortedDict[index];
            if (!s.StringStartsWith(item.Key)) continue;

            word = item;
            return true;
        }

        word = default;
        return false;
    }

    public static bool StringStartsWith(this string s0, string s1)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        int min = int.Min(s0.Length, s1.Length);
        for (int i = 0; i < min; i++)
            if (!s0[i].Equals(s1[i]))
                return false;

        return true;
    }
}