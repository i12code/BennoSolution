using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BenoSolution.Shared
{
    public class UploadManager
    {
        private readonly HttpClient _vimeoApiClient;
        private List<FileTransferInfo> _transfers = new();

        string postUrl = @"https://1512435599.cloud.vimeo.com/upload?ticket_id=419720307&video_file_id=2303689906&signature=6017bd368dfe1ac93ff7558dc3bf6958&v6=1&redirect_url=https%3A%2F%2Fvimeo.com%2Fupload%2Fapi%3Fvideo_file_id%3D2303689906%26app_id%3D162026%26ticket_id%3D419720307%26signature%3D708d37222acefcd264386ca4fa0f0a94fc0a19a2\";

        public IReadOnlyList<FileTransferInfo> Transfers => _transfers.AsReadOnly();

        public UploadManager(HttpClient vimeoApiClient)
        {
            _vimeoApiClient = vimeoApiClient;
        }

        public void StartFileUpload(Stream stream, String fileName, Int64 fileSize)
        {
            FileTransferInfo uploaderInfo = new FileTransferInfo(fileName, fileSize);
            uploaderInfo.Progress += UpdateFileProgressChanged;
            _transfers.Add(uploaderInfo);

            var singleFileContent = new ProgressableStreamContent(stream, uploaderInfo);
            //read the docs if vimeo expected such an encoded content
            var multipleFileContent = new MultipartFormDataContent();
            multipleFileContent.Add(singleFileContent, "file", fileName);

            Task.Run(async () =>
            {
                var result = await _vimeoApiClient.PostAsync(postUrl, multipleFileContent);
                uploaderInfo.ResponseReceived();

                uploaderInfo.Progress -= UpdateFileProgressChanged;
            });
        }

        private void UpdateFileProgressChanged(Object sender, FileTransferingProgressChangedEventArgs args) => FileTransferChanged?.Invoke(sender, args);

        public event EventHandler<FileTransferingProgressChangedEventArgs> FileTransferChanged;
    }
}
