using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JcNetworking;

namespace JohnnyController
{

    class JohnnyController
    {
        private int _time;
        private int _timeOverlap;
        private bool _running;
        private string _joystickName;
        private float[] _percents;
        private Joystick _js;
        private JcRobotNetworking _jcn;
        private float _controllerGain;
        private float _speed;
        private ControlState _controlState;

        private const int _numMotors = 15;

        private const int _servoWaist = 0;
        private const int _servoTorso = 1;
        private const int _servoHead = 2;
        private const int _servoShoulderLeft = 3;
        private const int _servoShoulderRight = 4;
        private const int _servoElbowLeft = 5;
        private const int _servoElbowRight = 6;
        private const int _servoArmRotationLeft = 7;
        private const int _servoArmRotationRight = 8;
        private const int _servoWristLeft = 9;
        private const int _servoWristRight = 10;
        private const int _servoHandLeft = 11;
        private const int _servoHandRight = 12;
        private const int _motorDriveLeft = 13;
        private const int _motorDriveRight = 14;
        private const int _motorTime = 254;

        private enum ControlState{
            Bottom, Top, Left, Right
        }

         static void Main(string[] args)
         {
            JohnnyController jc = new JohnnyController();
            jc.Start();

         }

        public JohnnyController(){

            _time = 50;
            _timeOverlap = 5;
            _speed = .1f;
            _running = true;
            _controllerGain = _speed / (1000 / _time);
            _jcn = new JcRobotNetworking(JcRobotNetworking.ConnectionType.Controller);
            _percents = new float[_numMotors];
            _controlState = ControlState.Bottom;
            Console.WriteLine("Choose a gampad:");
            Joystick.PrintJoysticks();
            _joystickName = Console.ReadLine();
            _joystickName = "/dev/input/" + _joystickName;

        }

        public void Start()
        {
            _js = new WeirdChineseController(_joystickName);
            _jcn.Connect(1296, "localhost");
            for (int i = 0; i < _percents.Length; i++){
                _percents[i] = .5f;
            }

            while (_running)
            {
                TestUpdate();
                Thread.Sleep(_time - _timeOverlap);
            }
        }

        public void TestUpdate(){
            AddValue(_servoHead,_js.GetThumbstickLeft().X);
            Console.WriteLine(_percents[_servoHead]);
            SendValues();
        }

        public void SendValues(){
            _jcn.SendCommand(new JcRobotNetworking.Command(_motorTime, BitConverter.GetBytes(_time)));
            for (byte i = 0; i < _percents.Length; i++){
                _jcn.SendCommand(new JcRobotNetworking.Command(i,BitConverter.GetBytes(_percents[i])));
            }
        }

        public void Update(){
            
            if (_js.GetButtonColorTop())
            {
                _controlState = ControlState.Top;
            }else if (_js.GetButtonColorBottom())
            {
                _controlState = ControlState.Bottom;
            }else if (_js.GetButtonColorLeft())
            {
                _controlState = ControlState.Left;
            }else if (_js.GetButtonColorRight())
            {
                _controlState = ControlState.Right;
            }

            switch(_controlState){
                case ControlState.Top: TopControl(); break;
                case ControlState.Bottom: BottomControl(); break;
                case ControlState.Left: LeftControl(); break;
                case ControlState.Right: RightControl(); break;
                default: break;
            }
            SendValues();
        }

        public void TopControl()
        {
            AddValue(_servoWaist, _js.GetThumbstickLeft().Y);
            AddValue(_servoTorso, _js.GetThumbstickRight().Y);
        }

        public void BottomControl()
        {
            SetValue(_motorDriveLeft, _js.GetThumbstickLeft().Y*.5f + _js.GetThumbstickLeft().X * .33f +.5f);
            SetValue(_motorDriveRight, _js.GetThumbstickLeft().Y*.5f - _js.GetThumbstickLeft().X * .33f + .5f);
            AddValue(_servoHead, _js.GetThumbstickRight().X);
        }

        public void LeftControl()
        {
            AddValue(_servoArmRotationLeft, _js.GetThumbstickLeft().X);
            AddValue(_servoHandLeft, (_js.GetTriggerRight()-_js.GetTriggerLeft()));
            AddValue(_servoElbowLeft, _js.GetThumbstickRight().Y);
            AddValue(_servoWristLeft, _js.GetThumbstickLeft().Y);
            AddValue(_servoShoulderLeft, _js.GetThumbstickRight().X);
        }

        public void RightControl()
        {
            AddValue(_servoArmRotationRight, _js.GetThumbstickLeft().X);
            AddValue(_servoHandRight, (_js.GetTriggerRight() - _js.GetTriggerLeft()));
            AddValue(_servoElbowRight, _js.GetThumbstickRight().Y);
            AddValue(_servoWristRight, _js.GetThumbstickLeft().Y);
            AddValue(_servoShoulderRight, _js.GetThumbstickRight().X);
        }

        private void AddValue(int index, float value)
        {
            _percents[index] += value* _controllerGain;
            _percents[index] = Minmax(_percents[index], 0, 1);
        }

        private void SetValue(int index, float value)
        {
            _percents[index] = value;
            _percents[index] = Minmax(_percents[index], 0, 1);
        }

        private float Minmax(float value, float min, float max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }

    }

   
}
