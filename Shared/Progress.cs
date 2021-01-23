using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BenoSolution.Shared
{
    //copied from https://stackoverflow.com/questions/35320238/how-to-display-upload-progress-using-c-sharp-httpclient-postasync
    public class ProgressableStreamContent : HttpContent
    {
        private const int defaultBufferSize = 4096;

        private readonly Stream _content;
        private readonly Int32 _bufferSize;
        private Boolean _contentConsumed;
        private readonly FileTransferInfo _progressInfo;

        public ProgressableStreamContent(Stream content, FileTransferInfo progressInfo) : this(content, defaultBufferSize, progressInfo) { }

        public ProgressableStreamContent(Stream content, Int32 bufferSize, FileTransferInfo progressInfo)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize));
            }

            this._content = content ?? throw new ArgumentNullException(nameof(content));
            this._bufferSize = bufferSize;
            this._progressInfo = progressInfo;
        }

        protected async override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            PrepareContent();

            var buffer = new Byte[this._bufferSize];

            _progressInfo.Start();

            using (_content)
            {
                while (true)
                {
                    var length = await _content.ReadAsync(buffer, 0, buffer.Length);
                    if (length <= 0) break;

                    _progressInfo.UpdateProgress(length);

                    stream.Write(buffer, 0, length);
                }
            }

            _progressInfo.UploadFinished();
        }

        protected override bool TryComputeLength(out long length)
        {
            length = _content.Length;
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _content.Dispose();
            }
            base.Dispose(disposing);
        }


        private void PrepareContent()
        {
            if (_contentConsumed)
            {
                // If the content needs to be written to a target stream a 2nd time, then the stream must support
                // seeking (e.g. a FileStream), otherwise the stream can't be copied a second time to a target 
                // stream (e.g. a NetworkStream).
                if (_content.CanSeek)
                {
                    _content.Position = 0;
                }
                else
                {
                    throw new InvalidOperationException("SR.net_http_content_stream_already_read");
                }
            }

            _contentConsumed = true;
        }
    }
}
