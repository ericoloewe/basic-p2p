namespace peer.Messages
{
    public enum PeerCommandType
    {
        // Default commands
        EXIT,
        GENERIC_ERROR,
        GET_KIND,
        KIND_OF_CONNECTION,
        STOP,

        // Peer commands
        CONNECTIONS,
        DOWNLOAD_FILE,
        DOWNLOAD_FILE_SLICE,
        GET_CONNECTIONS,
        GET_FILE_SLICE,
        GET_INFO,
        LIST_FILES,
        PEER_INFO,
        UPLOAD_FILE_SLICE,

        // Client commands
        GET_FILE,
        GET_LIST,
        UPLOAD_FILE,
    }
}