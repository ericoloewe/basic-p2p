﻿namespace peer.Messages
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
        FILE,
        GET_CONNECTIONS,
        LIST_FILES,
        DOWNLOAD_FILE,

        // Client commands
        GET_LIST,
        UPLOAD_FILE,
        GET_FILE,
    }
}