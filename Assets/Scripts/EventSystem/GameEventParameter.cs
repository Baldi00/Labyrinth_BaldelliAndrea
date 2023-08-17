public struct GameEventParameter
{
    public string Key;
    public object Value;

    public GameEventParameter(string key, object value)
    {
        this.Key = key;
        this.Value = value;
    }
}