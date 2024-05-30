using Infrastructure.External.ShopeeWebApi;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SNR_BGC
{
    public interface IAuthenthicationTokenProvider
    {
        Task<Infrastructure.External.ShopeeWebApi.AuthenticationToken> GetAuthenticationToken(CancellationToken cancellationToken);
    }

    public class AuthenthicationTokenProvider : IAuthenthicationTokenProvider
    {
        private readonly IAuthenthicationTokenFlowManager _authenthicationTokenFlowManager;

        private Infrastructure.External.ShopeeWebApi.AuthenticationToken? _currentToken;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private bool _disposed;

        public AuthenthicationTokenProvider(IAuthenthicationTokenFlowManager authenthicationTokenFlowManager)
        {
            _authenthicationTokenFlowManager = authenthicationTokenFlowManager;

        }

        public async Task<Infrastructure.External.ShopeeWebApi.AuthenticationToken> GetAuthenticationToken(CancellationToken cancellationToken)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(AuthenthicationTokenProvider));
            }
            await _semaphore.WaitAsync(cancellationToken);
            try
            {
                if (_currentToken == null)
                {
                    _currentToken = await _authenthicationTokenFlowManager.GetCurrent(cancellationToken);
                }
                if (_currentToken != null)
                {
                    if (_currentToken!.IsValid)
                    {
                        return _currentToken;
                    }
                    if (_currentToken!.CanRefresh)
                    {
                        _currentToken = await _authenthicationTokenFlowManager.Refresh(_currentToken, cancellationToken);
                        return _currentToken;
                    }
                }
                _currentToken = await _authenthicationTokenFlowManager.RequestNew(cancellationToken);
                return _currentToken;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _semaphore.Dispose();
                }
                _disposed = true;
            }
        }

    }
}