﻿using System.Linq;
using Assets.Scripts.World;
using UnityEngine;

public class PopularityEventBehaviour : WorldEventBehaviour
{
    private readonly Player _player;
    private readonly TestUnitFactory _unitFactory;
    private float _updatePeriod = 3f;
    private int _popularityStep = 2;
    private float _lastUpdateTime;
    private string _testFood = "Bread";

    #region constructors

    public PopularityEventBehaviour(GameWorld gameWorld,Player player,TestUnitFactory unitFactory) : 
        base(gameWorld)
    {
        _player = player;
        _unitFactory = unitFactory;
    }

    #endregion

    #region public methods

    public override void Update(float deltaTime)
    {
        _lastUpdateTime += deltaTime;
        if (_lastUpdateTime > _updatePeriod)
        {
            UpdatePopularity();
            _lastUpdateTime = 0;
        }
    }

    #endregion

    private void UpdatePopularity()
    {
        var city = _player.City;
        var stockpile = city.GetClosestStockpile(Vector3.zero);
        var citizensCount = city.FreeCitizensCount;
        var foodAmount = stockpile[_testFood];
        if (foodAmount < citizensCount)
        {
            _player.SetPopularity(_player.Popularity - _popularityStep);
        }
        else
        {
            _player.SetPopularity(_player.Popularity + _popularityStep);
        }
        if (_player.Popularity == 0)
        {
            RemoveCitizen(city);
        }
        else if (_player.Popularity>50 && city.FreeCitizensCount < 10)
        {
            _unitFactory.CreateUnit(_unitFactory.UnitInfos.FirstOrDefault(x => x.Name == "Peasant"));
        }
        stockpile.RemoveResource(_testFood,Mathf.Min(foodAmount,citizensCount));
    }

    private void RemoveCitizen(ICity city)
    {
        var citizen = city.GetFreeCitizen();
        if(citizen==null)return;
        _gameWorld.EntitiesBehaviour.RemoveEntity(citizen);
    }
}