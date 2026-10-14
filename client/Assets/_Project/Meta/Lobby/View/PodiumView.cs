using System.Collections.Generic;
using PlowParty.Meta.Lobby.Simulation;
using UnityEngine;

namespace PlowParty.Meta.Lobby.View
{
    public sealed class PodiumView : MonoBehaviour
    {
        [SerializeField] private Transform _row;
        [SerializeField] private GameObject _vehicleModel;
        [SerializeField] private GameObject[] _critterModels;
        [SerializeField] private int _capacity = 6;
        [SerializeField] private float _spacing = 1.9f;
        [SerializeField] private float _stageWidth = 4.6f;
        [SerializeField] private float _facing = 155f;

        private readonly List<PodiumSpot> _spots = new List<PodiumSpot>();

        public void Show(IReadOnlyList<MemberLook> looks)
        {
            var count = Mathf.Min(looks.Count, _capacity);
            _row.localScale = Vector3.one * PodiumLayout.Scale(count, _spacing, _stageWidth);
            for (var i = 0; i < _spots.Count; i++)
            {
                if (i < count)
                {
                    _spots[i].Show(looks[i], PodiumLayout.Offset(i, count, _spacing));
                }
                else
                {
                    _spots[i].Hide();
                }
            }
        }

        private void Awake()
        {
            for (var i = 0; i < _capacity; i++)
            {
                var vehicle = Instantiate(_vehicleModel, _row, false);
                vehicle.name = $"{_vehicleModel.name}_{i}";
                vehicle.transform.localRotation = Quaternion.Euler(0f, _facing, 0f);
                _spots.Add(new PodiumSpot(vehicle, _critterModels));
            }
        }
    }
}
