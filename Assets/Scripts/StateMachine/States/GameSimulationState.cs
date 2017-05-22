﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.World;
using Assets.Scripts.World.SocialModule;
using UnityEngine;

namespace Assets.Scripts.StateMachine.States
{
    public class GameSimulationState : State<GameState> {


        #region private properties

        private readonly TestWorldData _worldData;
        private OnGuiController _guiController;

        #endregion

        public GameSimulationState(IStateController<GameState> stateController,
            TestWorldData worldData) : 
            base(stateController)
        {
            _worldData = worldData;
        }

        public override IEnumerator Execute()
        {
            var world = InitializeSimulationWorld();
            
            while (true)
            {
                world.Update(Time.unscaledDeltaTime);
                yield return null;
            }
        }

        public override void Stop()
        {
            base.Stop();
            Object.DestroyImmediate(_guiController);
        }
        
        #region private methods

        private BaseWorld InitializeSimulationWorld()
        {
            //initialize world
            var playerRelationshp = new RelationshipMap(1);
            playerRelationshp.SetRelationship(0,10);
            playerRelationshp.SetRelationship(1, -1);
            var playerWorld = new PlayerWorld( playerRelationshp,
                _worldData.Fireplace.position);

            //init unit factory
            var unitFactory = _worldData.GetComponent<TestUnitFactory>();
            unitFactory.SetWorld(playerWorld);
            // create dummy stockpile
            var stockpile = unitFactory.
                CreateBuilding(unitFactory.BuildingInfos.First(info => info.Name == "Stockpile"));
            stockpile.SetPosition(Vector3.zero);
            playerWorld.Stockpile.AddStockpileBlock(stockpile as StockpileBlock);
            //create player
            var player = CreatePlayer(playerWorld);
            var popularityEventsBehaviour = new PopularityEvent(playerWorld, player, unitFactory);
            playerWorld.Events.Add(popularityEventsBehaviour);
            //init parent world
            var neutralRelationshipMap = new RelationshipMap(0);
            var world = new BaseWorld(neutralRelationshipMap,Vector3.zero);
            world.Children.Add(playerWorld);
            playerWorld.Parent = world;
            //initialize Temp OnGUI drawer
            _guiController = InitializeOnGuiDrawer(playerWorld,player,unitFactory);
            return world;
        }

        private Player CreatePlayer(BaseWorld world)
        {
            var playerInfo = new PlayerInfo();
            var player = new Player(playerInfo, world);
            return player;
        }

        private OnGuiController InitializeOnGuiDrawer(
            BaseWorld world,Player player, 
            TestUnitFactory unitFactory)
        {
            var guiObject = new GameObject("GuiConroller");
            var guiController = guiObject.AddComponent<OnGuiController>();
            guiController.Drawers.Add(new TestUnitOnGui(unitFactory));
            guiController.Drawers.Add(new PlayerDrawer(player));
            guiController.Drawers.Add(new ResourcesDrawer(world));
            //initialize additional events
            var selectionManager = new SelectionManager(world);
            var constructionModule = new ConstructionModule(world, unitFactory);
            world.Events.Add(constructionModule);
            var uiController = new ImUiController(world, selectionManager);
            guiController.Drawers.Add(constructionModule);
            guiController.Drawers.Add(selectionManager);
            guiController.Drawers.Add(uiController);
            return guiController;
        }

        #endregion
    }
}
