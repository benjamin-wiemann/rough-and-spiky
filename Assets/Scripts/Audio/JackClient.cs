using System;
using UnityEngine;
using System.Runtime.InteropServices;
//using static JackClient;
using System.Collections.Generic;

namespace Audio
{


    [RequireComponent(typeof(AudioSource))]
    public class JackClient : MonoBehaviour
    {


        [DllImport("libjack64.dll")]
        private static extern IntPtr jack_client_open(string clientName, JackOptions options, out JackStatus status);

        [DllImport("libjack64.dll")]
        private static extern IntPtr jack_client_close(IntPtr jackClient);

        [DllImport("libjack64.dll")]
        private static extern int jack_activate(IntPtr client);

        [DllImport("libjack64.dll")]
        private static extern IntPtr jack_port_register(
            IntPtr client,
            string portName,
            string portType,
            JackPortFlags flags,
            ulong bufferSize);

        [DllImport("libjack64.dll")]
        private static extern IntPtr jack_port_by_name(IntPtr client, string portName);

        [DllImport("libjack64.dll")]
        private static extern IntPtr jack_get_ports(IntPtr client, string portNamePattern, string typeNamePattern, JackPortFlags flags);

        [DllImport("libjack64.dll")]
        private static extern IntPtr jack_free(IntPtr pointerToArray);

        [DllImport("libjack64.dll")]
        private static extern int jack_connect(IntPtr client, string sourcePort, string destinationPort);

        [DllImport("libjack64.dll")]
        private static extern int jack_disconnect(IntPtr client, string sourcePort, string destinationPort);

        [DllImport("libjack64.dll")]
        private static extern IntPtr jack_port_get_connections(IntPtr port);

        [DllImport("libjack64.dll")]
        private static extern int jack_set_process_callback(IntPtr client, IntPtr callback, IntPtr arg);

        [DllImport("libjack64.dll")]
        private static extern IntPtr jack_get_client_name(IntPtr client);

        [DllImport("libjack64.dll")]
        private static extern IntPtr jack_port_get_buffer(IntPtr port, UInt32 nFrames);

        [DllImport("libjack64.dll")]
        private static extern UInt32 jack_get_buffer_size(IntPtr client);

        [DllImport("libjack64.dll")]
        private static extern UInt32 jack_get_sample_rate(IntPtr client);

        [DllImport("libjack64.dll")]
        private static extern IntPtr jack_ringbuffer_create(UInt32 size);

        [DllImport("libjack64.dll")]
        private static extern void jack_ringbuffer_free(IntPtr ringBuffer);

        [DllImport("libjack64.dll")]
        private static extern UInt32 jack_ringbuffer_write_space(IntPtr ringBuffer);

        [DllImport("libjack64.dll")]
        private static extern UInt32 jack_ringbuffer_read_space(IntPtr ringBuffer);

        [DllImport("libjack64.dll")]
        private static extern UInt32 jack_ringbuffer_write(IntPtr ringBuffer, IntPtr source, UInt32 cnt);

        [DllImport("libjack64.dll")]
        private static extern UInt32 jack_ringbuffer_read(IntPtr ringBuffer, IntPtr dest, UInt32 cnt);


        private enum JackOptions
        {
            JackNullOption = 0
        }

        [Flags]
        private enum JackStatus
        {
            JackFailure = 1,
            JackInvalidOption = 1 << 1,
            JackNameNotUnique = 1 << 2,
            JackServerStarted = 1 << 3,
            JackServerFailed = 1 << 4
        }

        [Flags]
        private enum JackPortFlags
        {
            JackPortIsInput = 1,
            JackPortIsOutput = 1 << 1,
            JackPortIsPhysical = 1 << 2,
            JackPortCanMonitor = 1 << 3,
            JackPortIsTerminal = 1 << 4
        }


        public delegate void CallbackDelegate(IntPtr samplesPerFrame, IntPtr arg);

        public string clientName = "UnityClient";
        public string leftInputPortName = "lInput";
        public string rightInputPortName = "rInput";

        public const string inputPortType = "32 bit float mono audio";

        private const UInt32 ringBufferSize = 16384;
        private IntPtr leftRingBuffer;
        private IntPtr rightRingBuffer;

        private IntPtr jackClientPtr;
        private string newClientName = "";
        private UInt32 bufferSize;

        private IntPtr leftPort;
        private IntPtr rightPort;

        private bool running = false;

        private void Start()
        {

            //GenerateDummyClip();
            OpenJackClient();

        }

        private void OpenJackClient()
        {
            JackStatus status;
            jackClientPtr = jack_client_open(clientName, JackOptions.JackNullOption, out status);

            if (status.HasFlag(JackStatus.JackServerStarted))
            {
                Debug.Log("Jack Server started.");
            }
            int sampleRate = AudioSettings.outputSampleRate;
            int jackSampleRate = (int)jack_get_sample_rate(jackClientPtr);
            if (sampleRate != jackSampleRate)
            {
                Debug.LogWarning($"Unity is running with a different sample rate than Jack. Unity: {sampleRate}, Jack: {jackSampleRate} ");
            }

            if (!status.HasFlag(JackStatus.JackFailure) && jackClientPtr != IntPtr.Zero)
            {
                Debug.Log("Jack client successfully opened.");
            }
            else
            {
                Debug.LogError("Failed to open Jack client.");
                return;
            }

            if (status.HasFlag(JackStatus.JackNameNotUnique))
            {
                newClientName = Marshal.PtrToStringAnsi(jack_get_client_name(jackClientPtr));
                Debug.Log($"Client name {clientName} is not unique, unique name {newClientName} assigned\n");
            }
            else
            {
                newClientName = clientName;
            }

            bufferSize = jack_get_buffer_size(jackClientPtr);

            RegisterPorts();

            leftRingBuffer = jack_ringbuffer_create(sizeof(float) * ringBufferSize);
            rightRingBuffer = jack_ringbuffer_create(sizeof(float) * ringBufferSize);

            //CallbackDelegate callbackDelegate = Process;
            //IntPtr processCallbackPtr = Marshal.GetFunctionPointerForDelegate(callbackDelegate);
            //jack_set_process_callback( jackClientPtr, processCallbackPtr, IntPtr.Zero);

            if (jack_activate(jackClientPtr) != 0)
            {
                Debug.LogError("Failed to activate JACK client.");
                return;
            }
            else
            {
                Debug.Log("Jack client activated.");
            }

            ConnectPorts();

            running = true;

        }

        private void RegisterPorts()
        {
            leftPort = jack_port_register(jackClientPtr,
            leftInputPortName,
            inputPortType,
            JackPortFlags.JackPortIsInput,
            bufferSize);

            rightPort = jack_port_register(jackClientPtr,
            rightInputPortName,
            inputPortType,
            JackPortFlags.JackPortIsInput,
            bufferSize);

        }

        private void ConnectPorts()
        {
            //string leftSystemOutputName = "capture_1";
            //string rightSystemOutputName = "capture_2";

            IntPtr portsPtr = jack_get_ports(jackClientPtr, "", "", JackPortFlags.JackPortIsOutput);
            List<string> portList = StringListFromCharPtrPtr(portsPtr);

            string lInput = $"{newClientName}:{leftInputPortName}";
            string rInput = $"{newClientName}:{rightInputPortName}";

            if (jack_connect(jackClientPtr, portList[0], lInput) == 0)
            {
                Debug.Log($"Connected port {portList[0]} to {lInput}.");
            }
            else
            {
                Debug.LogError("Failed to connect Unity client to JACK server.");
            }
            jack_free(portsPtr);

            printConnections(leftPort);
            printConnections(rightPort);

        }

        private void printConnections(IntPtr port)
        {
            IntPtr connections = jack_port_get_connections(port);
            List<string> connectionNames = StringListFromCharPtrPtr(connections);
            jack_free(connections);

            Debug.Log("Current connections: \n");
            foreach (string connection in connectionNames)
            {
                Debug.Log(connection);
            }
        }

        private static List<string> StringListFromCharPtrPtr(IntPtr charArrayPtrPtr)
        {

            // Create a list to hold the port names
            List<string> portList = new List<string>();

            if (charArrayPtrPtr != IntPtr.Zero)
            {
                // Iterate through the array of pointers until a null pointer is encountered
                IntPtr currentPtr = charArrayPtrPtr;
                while (currentPtr != IntPtr.Zero)
                {
                    // Get the pointer to the current port name string
                    IntPtr portPtr = Marshal.ReadIntPtr(currentPtr);

                    // Break the loop if a null pointer is encountered
                    if (portPtr == IntPtr.Zero)
                        break;

                    // Marshal the pointer to a string and add it to the list
                    string portName = Marshal.PtrToStringAnsi(portPtr);
                    portList.Add(portName);

                    // Move to the next pointer in the array
                    currentPtr += IntPtr.Size;
                }
            }

            return portList;
        }

        public void Process(IntPtr nFrames, IntPtr arg)
        {
            uint samplesPerFrame = bufferSize; // (uint) Marshal.ReadIntPtr(nFrames);
            if (!running)
            {
                return;
            }
            //IntPtr lPortBuffer = jack_port_get_buffer( leftPort, samplesPerFrame);
            //IntPtr rPortBuffer = jack_port_get_buffer( rightPort, samplesPerFrame);
            float[] data = new float[samplesPerFrame];
            for (int i = 0; i < samplesPerFrame; i++)
            {
                data[i] = (float)Mathf.Sin((float)i * Mathf.PI / (float)samplesPerFrame);
            }

            IntPtr lPortBuffer = Marshal.AllocHGlobal((int)samplesPerFrame * sizeof(float));
            IntPtr rPortBuffer = Marshal.AllocHGlobal((int)samplesPerFrame * sizeof(float));
            Marshal.Copy(data, 0, lPortBuffer, (int)samplesPerFrame);
            Marshal.Copy(data, 0, rPortBuffer, (int)samplesPerFrame);

            if (jack_ringbuffer_write_space(leftRingBuffer) >= samplesPerFrame * sizeof(float))
            {
                jack_ringbuffer_write(leftRingBuffer, lPortBuffer, samplesPerFrame * sizeof(float));
            }
            if (jack_ringbuffer_write_space(rightRingBuffer) >= samplesPerFrame * sizeof(float))
            {
                jack_ringbuffer_write(rightRingBuffer, rPortBuffer, samplesPerFrame * sizeof(float));
            }
            Marshal.FreeHGlobal(lPortBuffer);
            Marshal.FreeHGlobal(rPortBuffer);
        }

        private void OnDestroy()
        {
            if (jackClientPtr != IntPtr.Zero)
            {
                jack_disconnect(jackClientPtr, $"{newClientName}:{leftInputPortName}", $"{newClientName}:{rightInputPortName}");
                jack_client_close(jackClientPtr);
            }
            if (leftRingBuffer != IntPtr.Zero)
            {
                jack_ringbuffer_free(leftRingBuffer);
            }
            if (rightRingBuffer != IntPtr.Zero)
            {
                jack_ringbuffer_free(rightRingBuffer);
            }
        }

        void GenerateDummyClip()
        {
            AudioClip clip = AudioClip.Create("clip", (int)bufferSize, 1, (int)jack_get_sample_rate(jackClientPtr), false);
            AudioSource source = GetComponent<AudioSource>();

            float[] samples = new float[bufferSize];
            System.Array.Clear(samples, 0, samples.Length);
            clip.SetData(samples, 0);

            source.clip = clip;
            source.loop = true;
            source.Play();
        }

        private void OnAudioFilterRead(float[] data, int nChannels)
        {
            if (!running)
                return;
            if (data.Length != bufferSize)
            {
                Debug.LogError($"Buffer size ({data.Length}) does not match Jack buffer size ({bufferSize}).");
                return;
            }
            if (nChannels != 2)
            {
                Debug.LogError("Jack expects stereo signal.");
                return;
            }

            float[] dest = new float[bufferSize * sizeof(float) * nChannels];
            Debug.Log($"Ringbuffer read space: {jack_ringbuffer_read_space(leftRingBuffer)} write space: {jack_ringbuffer_write_space(leftRingBuffer)} Needed space: {data.Length * sizeof(float)}");
            if (jack_ringbuffer_read_space(leftRingBuffer) >= data.Length * sizeof(float))
            {
                IntPtr leftDestPtr = Marshal.AllocHGlobal((int)bufferSize * sizeof(float));
                jack_ringbuffer_read(leftRingBuffer, leftDestPtr, bufferSize);
                Marshal.Copy(leftDestPtr, dest, 0, (int)bufferSize);
            }
            if (jack_ringbuffer_write_space(rightRingBuffer) >= data.Length * sizeof(float))
            {
                IntPtr rightDestPtr = Marshal.AllocHGlobal((int)bufferSize * sizeof(float));
                jack_ringbuffer_read(rightRingBuffer, rightDestPtr, bufferSize);
                Marshal.Copy(rightDestPtr, dest, (int)bufferSize, (int)bufferSize);
            }

            // Put the data samples in alternating order
            for (int i = 0; i < data.Length / 2; i++)
            {
                if (i % 2 == 0)
                    data[i] = dest[i / 2];
                else
                    if (i < data.Length)
                    data[i] = dest[i - 1 + (data.Length / 2)];
                else
                    data[i] = dest[i];
            }

        }
    }
}