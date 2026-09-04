using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Airox.Client.Core;

namespace Airox.Client.Networking
{
    public sealed class AiroxUnityRealtimeClient : MonoBehaviour
    {
        [SerializeField] private AiroxClientConfig config;
        public event Action<string> SnapshotReceived;
        public event Action<string> StatusChanged;
        public event Action<string> CombatAcknowledged;
        private ClientWebSocket socket;
        private CancellationTokenSource cts;
        private int sequence;
        public bool IsConnected => socket != null && socket.State == WebSocketState.Open;

        public async void Connect()
        {
            if (config == null || string.IsNullOrWhiteSpace(config.matchId) || string.IsNullOrWhiteSpace(config.accessToken))
            { StatusChanged?.Invoke("Configure matchId and accessToken"); return; }
            if (IsConnected) return;
            try
            {
                socket = new ClientWebSocket();
                socket.Options.AddSubProtocol("airox.v1");
                socket.Options.AddSubProtocol(config.accessToken);
                cts = new CancellationTokenSource();
                var uri = new Uri(config.websocketBaseUrl.TrimEnd('/') + "/ws/v1/matches/" + config.matchId);
                StatusChanged?.Invoke("Connecting...");
                await socket.ConnectAsync(uri, cts.Token);
                StatusChanged?.Invoke("Connected");
                _ = ReceiveLoop(cts.Token);
            }
            catch (Exception ex) { StatusChanged?.Invoke("Connection failed: " + ex.Message); await Disconnect(); }
        }

        public Task SendInput(float moveX, float moveZ, bool sprint, bool jump) =>
            SendInputWithSequence(NextSequence(), moveX, moveZ, sprint, jump);

        public int ReserveInputSequence() => NextSequence();

        public Task SendInputWithSequence(int inputSequence, float moveX, float moveZ, bool sprint, bool jump) =>
            SendText($"{{\"type\":\"input\",\"inputSequence\":{inputSequence},\"moveX\":{F(moveX)},\"moveZ\":{F(moveZ)},\"sprint\":{Bool(sprint)},\"jump\":{Bool(jump)}}}");

        public Task SendAttack(string targetPlayerId, string weaponId, Vector3 aim) =>
            SendText($"{{\"type\":\"attack\",\"inputSequence\":{NextSequence()},\"targetPlayerId\":\"{Escape(targetPlayerId)}\",\"weaponId\":\"{Escape(weaponId)}\",\"aimX\":{F(aim.x)},\"aimY\":{F(aim.y)},\"aimZ\":{F(aim.z)}}}");

        public Task SendReload(string weaponId) =>
            SendText($"{{\"type\":\"reload\",\"inputSequence\":{NextSequence()},\"weaponId\":\"{Escape(weaponId)}\"}}");

        public Task SendWeaponSwitch(string weaponId) =>
            SendText($"{{\"type\":\"weapon_switch\",\"inputSequence\":{NextSequence()},\"weaponId\":\"{Escape(weaponId)}\"}}");

        private int NextSequence() => Interlocked.Increment(ref sequence);
        private async Task SendText(string text)
        {
            if (!IsConnected) return;
            var bytes = Encoding.UTF8.GetBytes(text);
            try { await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cts.Token); }
            catch (Exception ex) { StatusChanged?.Invoke("Send failed: " + ex.Message); }
        }
        private async Task ReceiveLoop(CancellationToken token)
        {
            var buffer = new byte[16384];
            try
            {
                while (socket != null && socket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    using var ms = new System.IO.MemoryStream();
                    WebSocketReceiveResult result;
                    do
                    {
                        result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), token);
                        if (result.MessageType == WebSocketMessageType.Close) break;
                        ms.Write(buffer, 0, result.Count);
                    } while (!result.EndOfMessage);
                    if (result.MessageType == WebSocketMessageType.Close) break;
                    var message = Encoding.UTF8.GetString(ms.ToArray());
                    if (message.IndexOf("ack", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("hit", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("reload", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("weapon_switch", StringComparison.OrdinalIgnoreCase) >= 0)
                        CombatAcknowledged?.Invoke(message);
                    SnapshotReceived?.Invoke(message);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { StatusChanged?.Invoke("Realtime error: " + ex.Message); }
            finally { StatusChanged?.Invoke("Disconnected"); }
        }
        public async Task Disconnect()
        {
            try { cts?.Cancel(); if (socket != null && socket.State == WebSocketState.Open) await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "client shutdown", CancellationToken.None); }
            catch { }
            socket?.Dispose(); socket = null; cts?.Dispose(); cts = null;
        }
        private static string F(float v) => v.ToString(System.Globalization.CultureInfo.InvariantCulture);
        private static string Bool(bool v) => v ? "true" : "false";
        private static string Escape(string value) => (value ?? string.Empty).Replace("\\", "\\\\").Replace("\"", "\\\"");
        private async void OnDestroy() => await Disconnect();
    }
}
