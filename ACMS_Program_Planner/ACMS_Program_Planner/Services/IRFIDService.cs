using System;
using System.Threading.Tasks;
using ACMS_Program_Planner.Models;

namespace ACMS_Program_Planner.Services
{
    public interface IRFIDService
    {
        event Action<string>? DataReceived;
        event Action<string>? ErrorOccurred;
        event Action<bool>? ConnectionChanged;

        bool IsConnected { get; }

        void Connect(SettingsModel settings);
        void Disconnect();
        void Cleanup();

        void SendData(string? data);
        Task<string> SendCommandAndWaitAsync(string command, int expectedReplyLength);

        RFIDCommandTWN4 Command { get; }
    }
}
