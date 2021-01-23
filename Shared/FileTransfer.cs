using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenoSolution.Shared
{
    public class FileTransferingProgressChangedEventArgs : EventArgs
    {
        public String Filename { get; set; }
        public Int64 BytesSent { get; init; }
        public Int64 TotalBytes { get; init; }
        public FileTransferInfo.States State { get; init; }
    }

    public class FileTransferInfo
    {
        public enum States
        {
            Init,
            Pending,
            Transfering,
            PendingResponse,
            Finished
        }

        public States State { get; private set; }
        public Int64 BytesSent { get; private set; }
        public String Filename { get; private set; }
        public Int64 TotalSize { get; private set; }

        public FileTransferInfo(String filename, Int64 totalSize)
        {
            Filename = filename;
            TotalSize = totalSize;
            BytesSent = 0;
            State = States.Init;
        }

        private void SendEventHandler() => Progress?.Invoke(this,
            new FileTransferingProgressChangedEventArgs
            {
                BytesSent = BytesSent,
                State = State,
                TotalBytes = TotalSize,
                Filename = Filename
            });

        public event EventHandler<FileTransferingProgressChangedEventArgs> Progress;

        public void Start()
        {
            State = States.Transfering;
            SendEventHandler();
        }

        public void UpdateProgress(int length)
        {
            State = States.Transfering;
            BytesSent += length;
            SendEventHandler();
        }

        internal void UploadFinished()
        {
            State = States.PendingResponse;
            //just in case
            BytesSent = TotalSize;
            SendEventHandler();
        }

        internal void ResponseReceived()
        {
            State = States.Finished;
            SendEventHandler();
        }
    }
}
