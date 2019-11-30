namespace peer.Messages
{
    public enum PeerCommandType
    {
        // Default commands
        EXIT,
        GENERIC_ERROR,
        STOP,

        // Peer commands
        CONNECTIONS,
        FILE,
        GET_CONNECTIONS,
        LIST_FILES,

        // Client commands
        GET_LIST,
        UPLOAD_FILE,
    }
}