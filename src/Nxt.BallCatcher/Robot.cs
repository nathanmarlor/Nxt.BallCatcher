using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NKH.MindSqualls;
using System.Threading;

namespace Nxt.BallCatcher
{
    class Robot
    {
        private NxtBrick brick;
        private NxtUltrasonicSensor ultrasonicSensor;
        private Nxt2ColorSensor colorSensor;
        private const int threshold = 240;
        private const int sampleCount = 3;
        private readonly List<int> sonarValues = new List<int>();
        public ManualResetEvent ResetEvent = new ManualResetEvent(false);

        public int UltraSonicValue
        {
            get
            {
                return (int)sonarValues.Average();
            }
        }

        public void Init()
        {
            brick = new NxtBrick(NxtCommLinkType.Bluetooth, byte.Parse("40"));

            Console.WriteLine("Initialised brick on COM40");

            brick.MotorA = new NxtMotor();
            brick.MotorB = new NxtMotor();
            brick.MotorC = new NxtMotor();

            ultrasonicSensor = new NxtUltrasonicSensor();
            colorSensor = new Nxt2ColorSensor();

            brick.Sensor3 = colorSensor;
            brick.Sensor4 = ultrasonicSensor;

            ultrasonicSensor.PollInterval = 50;
            ultrasonicSensor.OnPolled += new Polled(ultrasonicSensor_OnPolled);

            colorSensor.PollInterval = 150;
            colorSensor.OnPolled += new Polled(colorSensor_OnPolled);

            brick.Connect();

            Console.WriteLine("Connected to brick!");
        }

        public void MoveAndStop(Action<Robot> action, int length)
        {
            action(this);
            Thread.Sleep(length);
            Stop();
        }

        public void Forward(sbyte power)
        {
            brick.MotorC.Run((sbyte)(power), 0);
            brick.MotorB.Run((sbyte)(power), 0);
        }

        public void Back(sbyte power)
        {
            brick.MotorC.Run((sbyte)(-power), 0);
            brick.MotorB.Run((sbyte)(-power), 0);
        }

        public void Left(sbyte power)
        {
            brick.MotorC.Run((sbyte)(-power), 0);
            brick.MotorB.Run((sbyte)(power), 0);
        }

        public void Right(sbyte power)
        {
            brick.MotorC.Run((sbyte)(power), 0);
            brick.MotorB.Run((sbyte)(-power), 0);
        }

        public void Stop()
        {
            brick.MotorB.Idle();
            brick.MotorC.Idle();
            Thread.Sleep(200);
        }

        public void OpenJaw()
        {
            brick.MotorA.Run(25, 0);
            Thread.Sleep(2000);
            brick.MotorA.Idle();
        }

        public void CloseJaw()
        {
            brick.MotorA.Run(-25, 0);
        }

        private void ultrasonicSensor_OnPolled(NxtPollable polledItem)
        {
            NxtUltrasonicSensor ultraSensor = (NxtUltrasonicSensor)polledItem;

            if (sonarValues.Count == sampleCount)
            {
                sonarValues.RemoveAt(0);
            }
            sonarValues.Add(ultraSensor.DistanceCm ?? 0);

            if (UltraSonicValue < threshold)
            {
                ResetEvent.Set();
            }
            else
            {
                ResetEvent.Reset();
            }

            Console.WriteLine("Ultra: " + ultraSensor.DistanceCm);
        }

        private void colorSensor_OnPolled(NxtPollable polledItem)
        {
			colorSensor = (Nxt2ColorSensor)polledItem;

            Console.WriteLine("Found colour: " + colorSensor.Color);
            if (colorSensor.Color == Nxt2Color.White)
            {
                Console.WriteLine("Found WHITE - going back to avoid");
                Stop();
                MoveAndStop(r => r.Back(50), 1000);
            }
        }
    }
}
