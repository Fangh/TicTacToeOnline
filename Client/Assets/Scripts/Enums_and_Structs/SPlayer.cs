[System.Serializable]
public struct SPlayer
{
    public string id;
    public string token;

    public SPlayer(string _id, string _token)
    {
        id = _id;
        token = _token;
    }

    public SPlayer(string _id)
    {
        id = _id;
        token = "";
    }
}
