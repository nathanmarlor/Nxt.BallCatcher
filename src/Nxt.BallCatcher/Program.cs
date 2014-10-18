using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NKH.MindSqualls;
using System.Threading;

namespace Nxt.BallCatcher
{
    class Program
    {
        private static Robot robot;
        private const int threshold = 240;

        public static void Main(string[] args)
        {
            Console.WriteLine("Starting NXT Ball Catcher");

            robot = new Robot();

            robot.Init();

            FindBall();
            Console.WriteLine("Found object!");

            while (true)
            {
                if (!ApproachBall())
                {
                    FindBall();
                    continue;
                }

                CloseJaw();
            }
        }

        private static void CloseJaw()
        {
            robot.MoveAndStop(r => r.Forward(35), 200);

            robot.CloseJaw();
            Thread.Sleep(1000);

            Console.WriteLine("Celebrating!");

            robot.OpenJaw();

            robot.MoveAndStop(r => r.Back(35), 1100);
        }

        private static bool ApproachBall()
        {
            for (int i = 0; i < 2; i++)
            {
                Console.WriteLine("Attempting to approach ball for " + i + " time");
                while (robot.UltraSonicValue < threshold)
                {
                    i = 0;
                    if (robot.UltraSonicValue < 12)
                    {
                        Console.WriteLine("Ready to grab!");
                        return true;
                    }

                    robot.MoveAndStop(r => r.Forward(30), 175);
                }

                if (Look(r => r.Right(8), 1250, r => r.Left(8)))
                {
                    Console.WriteLine("Found! Resetting search count");
                    i = 0;
                    continue;
                }

                if (Look(r => r.Left(8), 1250, r => r.Right(8)))
                {
                    Console.WriteLine("Found! Resetting search count");
                    i = 0;
                    continue;
                }

                //robot.MoveAndStop(r => r.Back(50), 250);
            }

            Console.WriteLine("Object has disapeared!");
            return false;
        }

        private static void FindBall()
        {
            while (true)
            {
                if (Look(r => r.Forward(50), 500))
                {
                    break;
                }

                if (Look(r => r.Left(20), 750, r => r.Right(20)))
                {
                    break;
                }

                if (Look(r => r.Right(20), 750, r => r.Left(20)))
                {
                    break;
                }
            }
        }

        private static bool Look(Action<Robot> action, int length, Action<Robot> correction = null)
        {
            action(robot);
            if (robot.ResetEvent.WaitOne(length))
            {
                robot.Stop();
                return true;
            }
            robot.Stop();

            if (correction != null)
            {
                correction(robot);
                if (robot.ResetEvent.WaitOne(length))
                {
                    robot.Stop();
                    return true;
                }
                robot.Stop();
            }

            return false;
        }
    }
}
