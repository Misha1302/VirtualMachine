namespace Tokenizer;

using Tokenizer.Token;

public static class Extension
{
    public static bool StartsWithAny(this string s, IReadOnlyDictionary<string, TokenType> words,
        out KeyValuePair<string?, TokenType> word)
    {
        List<KeyValuePair<string?, TokenType>> sortedDict =
            words.OrderBy(x => x.Key.Length).ToList();

        for (int index = sortedDict.Count - 1; index >= 0; index--)
        {
            KeyValuePair<string?, TokenType> item = sortedDict[index];
            if (!s.StartsWith(item.Key)) continue;
            
            word = item;
            return true;
        }

        word = default;
        return false;
    }
}