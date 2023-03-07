using Microsoft.Xna.Framework;
using System;
using System.Timers;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace RandomMathProblemPlugin
{
    [ApiVersion(2, 1)]
    public class RandomMathProblemPlugin : TerrariaPlugin
    {
        private Timer _timer;
        private Random _random = new Random();
        private int _currentProblemAnswer = 0;
        private string _currentProblemDescription = string.Empty;
        private bool _isProblemSolved = true;
        private int _countMoney = 10; // значение по умолчанию
        public int CountMoney
        {
            get { return _countMoney; }
            set
            {
                if (_countMoney != value)
                {
                    _countMoney = value;
                    TShock.Utils.Broadcast($"Вознаграждение за решение установлено в {CountMoney} золотых монет.", Color.Cyan);
                }
            }
        }

        public override string Name => "RandomMathProblemPlugin";
        public override string Author => "YourName";
        public override string Description => "Random math problem plugin";
        public override Version Version => new Version(1, 0, 0, 0);

        public RandomMathProblemPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(SetCountMoney, "setmoney") {HelpText = "/setmoney <value> устанавливает вознаграждение за решение задачи" });
            ServerApi.Hooks.ServerChat.Register(this, OnChatMessage);
            _timer = new Timer(60 * 1000); // 60 seconds
            _timer.Elapsed += GenerateNewProblem;
            _timer.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
            }
            base.Dispose(disposing);
        }
        private void SetCountMoney(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Player.SendErrorMessage("Использование: /setmoney <value>");
                return;
            }

            if (!int.TryParse(args.Parameters[0], out int value))
            {
                args.Player.SendErrorMessage("Неверное значение.");
                return;
            }

            CountMoney = value;
        }

        private void GenerateNewProblem(object sender, ElapsedEventArgs e)
        {
            if (!_isProblemSolved)
                return;

            int firstNumber = 0;
            int secondNumber = 0;
            string operation = "";
            int answer = 0;
            int operationIndex = _random.Next(0, 4);
            switch (operationIndex)
            {
                case 0:
                    firstNumber = _random.Next(1, 101);
                    secondNumber = _random.Next(1, 101);
                    operation = "+";
                    answer = firstNumber + secondNumber;
                    break;
                case 1:
                    firstNumber = _random.Next(1, 101);
                    secondNumber = _random.Next(1, 101);
                    operation = "-";
                    answer = firstNumber - secondNumber;
                    break;
                case 2:
                    firstNumber = _random.Next(1, 11);
                    secondNumber = _random.Next(1, 11);
                    operation = "*";
                    answer = firstNumber * secondNumber;
                    break;
                case 3:
                    firstNumber = _random.Next(1, 11);
                    secondNumber = _random.Next(1, 11);
                    firstNumber *= secondNumber;
                    operation = "/";
                    answer = firstNumber / secondNumber;

                    break;
            }

            _currentProblemAnswer = answer;
            _isProblemSolved = false;

            _currentProblemDescription = $"Кто первые решит задачу получит {CountMoney} золотых монет: {firstNumber} {operation} {secondNumber} {answer}";

            TShock.Utils.Broadcast(_currentProblemDescription, Color.Cyan);
        }

        private void OnChatMessage(ServerChatEventArgs args)
        {
            if (_isProblemSolved)
                return;
            if (args.Text.StartsWith("/"))
                return;
            if (!int.TryParse(args.Text, out int answer))
                return;

            var player = TShock.Players[args.Who];
            if (player == null)
                return;

            if (answer == _currentProblemAnswer)
            {
                _isProblemSolved = true;
                player.GiveItem(73, CountMoney);
                TShock.Utils.Broadcast($"{player.Name} первый решил задачу и получает {CountMoney} золотых монет!", Color.Cyan);
            }
        }
    }
}
