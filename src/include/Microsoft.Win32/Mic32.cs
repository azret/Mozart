namespace Microsoft.WinMM {
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.Win32;

    public sealed class Mic32 : IDisposable {
        public struct Stereo {
            public float CH1;
            public float CH2;
        }

        private object _lock = new object();
        private IntPtr _hwih;
        private WinMM.WaveInProc _hwiproc;
        private int _cc;

        public Mic32(int cc, int nSamplesPerSec, Action<Mic32, IntPtr> onReady) {
            this._cc = cc;
            this._data = new Stereo[cc];
            this._wfx = new WinMM.WaveFormatEx();
            this._wfx.wBitsPerSample = 16;
            this._wfx.nChannels = 2;
            this._wfx.nBlockAlign = (short)(_wfx.nChannels * _wfx.wBitsPerSample / 8);
            this._wfx.wFormatTag = (short)WinMM.WaveFormatTag.Pcm;
            this._wfx.nSamplesPerSec = nSamplesPerSec;
            this._wfx.nAvgBytesPerSec = _wfx.nSamplesPerSec * _wfx.nBlockAlign;
            this._wfx.cbSize = 0;
            this._hwiproc = new WinMM.WaveInProc((IntPtr waveInHandle, WinMM.WaveInMessage message,
                        IntPtr instance, IntPtr wh, IntPtr param2) => {
                if (onReady != null && message == WinMM.WaveInMessage.DataReady) {
                    onReady(this, wh);
                }
            });
        }

        Stereo[] _data;

        public unsafe void CaptureData(WaveHeader* pwh, short* psData) {
            lock (_lock) {
                for (int s = 0; s < _cc; s++) {
                    float ch1 = (psData[(s * Channels)] / 32767.0f),
                        ch2 = (psData[(s * Channels) + 1] / 32767.0f);
                    _data[s].CH1 = (float)ch1;
                    _data[s].CH2 = (float)ch2;
                }
            }
        }

        public Stereo[] ReadData() {
            Stereo[] local;
            lock (_lock) {
                local = (Stereo[])_data.Clone();
            }
            return local;
        }

        ~Mic32() {
            this.Dispose(false);
        }

        WinMM.WaveFormatEx _wfx;

        public int Hz {
            get {
                return _wfx.nSamplesPerSec;
            }
        }

        public int Channels {
            get {
                return _wfx.nChannels;
            }
        }

        public void Open(bool muted = true) {
            const int WaveInMapperDeviceId = -1;
            if (this._hwih != IntPtr.Zero) {
                throw new InvalidOperationException("The device is already open.");
            }
            IntPtr h = new IntPtr();
            WinMM.Throw(
                WinMM.waveInOpen(
                    ref h,
                    WaveInMapperDeviceId,
                    ref _wfx,
                    this._hwiproc,
                    (IntPtr)0,
                    WinMM.WaveOpenFlags.CALLBACK_FUNCTION | WinMM.WaveOpenFlags.WAVE_FORMAT_DIRECT),
                WinMM.ErrorSource.WaveIn);
            this._hwih = h;
            this.AllocateHeaders();
            _isMuted = true;
            if (!muted) {
                WinMM.Throw(
                   WinMM.waveInStart(this._hwih),
                   WinMM.ErrorSource.WaveIn);
                _isMuted = false;
            }
        }

        public void Close() {
            this.FreeHeaders();
            if (this._hwih == IntPtr.Zero) return;
            _isMuted = true;
            WinMM.waveInClose(this._hwih);
            this._hwih = IntPtr.Zero;
        }

        IntPtr[] _headers = new IntPtr[32];

        unsafe void FreeHeaders() {
            for (int i = 0; i < _headers.Length; i++) {
                WaveHeader* pwh = (WaveHeader*)_headers[i];
                Marshal.FreeHGlobal((*pwh).lpData);
                Marshal.FreeHGlobal((IntPtr)pwh);
                _headers[i] = IntPtr.Zero;
            }
        }

        unsafe void AllocateHeaders() {
            if (this._hwih == IntPtr.Zero) {
                throw new InvalidOperationException();
            }
            for (int i = 0; i < _headers.Length; i++) {
                if (_headers[i] == IntPtr.Zero) {
                    WaveHeader* pwh = (WaveHeader*)Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WaveHeader)));
                    (*pwh).dwFlags = 0;
                    (*pwh).dwBufferLength = _cc
                        * _wfx.nBlockAlign;
                    (*pwh).lpData = Marshal.AllocHGlobal((*pwh).dwBufferLength);
                    (*pwh).dwUser = IntPtr.Zero;
                    _headers[i] = (IntPtr)pwh;
                    WinMM.Throw(
                        WinMM.waveInPrepareHeader(this._hwih, _headers[i], Marshal.SizeOf(typeof(WaveHeader))),
                        WinMM.ErrorSource.WaveOut);
                    WinMM.Throw(
                        WinMM.waveInAddBuffer(this._hwih, _headers[i], Marshal.SizeOf(typeof(WaveHeader))),
                        WinMM.ErrorSource.WaveOut);
                }
            }
        }

        public IntPtr Handle { get => _hwih; }

        public int Samples { get => _cc; }

        unsafe void UnPrepareHeaders() {
            if (this._hwih == IntPtr.Zero) {
                throw new InvalidOperationException();
            }
            for (int i = 0; i < _headers.Length; i++) {
                WaveHeader* pwh = (WaveHeader*)_headers[i];
                if (pwh != null && (((*pwh).dwFlags & WaveHeaderFlags.Prepared) == WaveHeaderFlags.Prepared)) {
                    WinMM.Throw(
                        WinMM.waveInUnprepareHeader(this._hwih, _headers[i], Marshal.SizeOf(typeof(WaveHeader))),
                        WinMM.ErrorSource.WaveOut);
                }
            }
        }

        bool _isMuted = true;

        public void Toggle() {
            if (!_isMuted) {
                _isMuted = true;
                WinMM.Throw(
                    WinMM.waveInStop(this._hwih),
                    WinMM.ErrorSource.WaveIn);
            } else {
                WinMM.Throw(
                   WinMM.waveInStart(this._hwih),
                   WinMM.ErrorSource.WaveIn);
                _isMuted = false;
            }
        }

        public void Mute() {
            _isMuted = true;
            WinMM.Throw(
                WinMM.waveInStop(this._hwih),
                WinMM.ErrorSource.WaveIn);
        }

        public void UnMute() {
            WinMM.Throw(
               WinMM.waveInStart(this._hwih),
               WinMM.ErrorSource.WaveIn);
            _isMuted = false;
        }

        public void Dispose() {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing) {
            if (this._hwih != null) {
                this.Close();
            }
        }

        #region Events

        private IntPtr[] _handles;

        public void AddHandle(IntPtr hWnd) {
            lock (_lock) {
                if (_handles == null) {
                    _handles = new IntPtr[0];
                }
                Array.Resize(ref _handles,
                    _handles.Length + 1);
                _handles[_handles.Length - 1] = hWnd;
            }
        }

        public void ClearHandles() {
            lock (_lock) {
                _handles = null;
            }
        }

        public void RemoveHandle(IntPtr hWnd) {
            lock (_lock) {
                if (_handles != null) {
                    for (int i = 0; i < _handles.Length; i++) {
                        if (_handles[i] == hWnd) {
                            _handles[i] = IntPtr.Zero;
                        }
                    }
                }
            }
        }

        public unsafe void Notify(Microsoft.WinMM.Mic32 hMic, IntPtr hWaveHeader) {
            lock (_lock) {
                if (_handles == null) {
                    return;
                }
                foreach (IntPtr hWnd in _handles) {
                    if (hWnd != IntPtr.Zero) {
                        User32.PostMessage(hWnd, WM.WINMM, hMic.Handle,
                            hWaveHeader);
                    }
                }
            }
        }
        #endregion
    }
}