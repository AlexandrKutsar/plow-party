using System;
using Fusion;
using PlowParty.Infrastructure.Network;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;

namespace PlowParty.Gameplay.Vehicle.Network
{
    public sealed class VehicleInputPoller : IStartable, IDisposable
    {
        private const string MoveActionPath = "Player/Move";
        private const string GadgetActionPath = "Player/Attack";

        private readonly NetworkRunnerEvents _events;
        private InputAction _move;
        private InputAction _gadget;

        public VehicleInputPoller(NetworkRunnerEvents events)
        {
            _events = events;
        }

        public void Start()
        {
            _move = InputSystem.actions.FindAction(MoveActionPath, true);
            _gadget = InputSystem.actions.FindAction(GadgetActionPath, true);
            _move.Enable();
            _gadget.Enable();
            _events.InputRequested += OnInputRequested;
        }

        public void Dispose()
        {
            _events.InputRequested -= OnInputRequested;
        }

        private void OnInputRequested(NetworkRunner runner, NetworkInput input)
        {
            input.Set(new VehicleNetworkInput
            {
                Move = _move.ReadValue<Vector2>(),
                GadgetPressed = _gadget.IsPressed(),
            });
        }
    }
}
