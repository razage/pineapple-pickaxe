using Godot;
using PineapplePickaxe.Generators.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace PineapplePickaxe.Generators
{
	public class MapGenerator : TileMap
	{
		[Signal] public delegate void LevelGenerated(Vector2 spawnPoint);

		[Export] public Vector2 mapSize = new Vector2(50, 50);
		[Export] public float walkerSpawnChance = 0.005f;
		[Export] public float randomDirectionChance = 0.4f;
		[Export] public int maxWalkers = 10;
		[Export] public float percentageToFill = 0.35f;
		[Export] public int paddingFromWall = 2; // This effectively is divided by 2 and applied to each side evenly
		[Export] public int iterations = 10000;

		private List<Vector2> floorTiles = new List<Vector2>();
		private List<Vector2> rockTiles = new List<Vector2>();
		private List<Walker> walkers = new List<Walker>();
		private Node rockLayer = new Node();
		// These are most likely temporary
		private PackedScene[] rocks = { (PackedScene)ResourceLoader.Load("res://src/Objects/Rock1.tscn"), (PackedScene)ResourceLoader.Load("res://src/Objects/Rock2.tscn"), (PackedScene)ResourceLoader.Load("res://src/Objects/Rock3.tscn") };
		private PackedScene basicOre = (PackedScene)ResourceLoader.Load("res://src/Objects/Ore1.tscn");
		private RandomNumberGenerator rng = new RandomNumberGenerator();
		private Vector2[] directions = new Vector2[4] { Vector2.Up, Vector2.Right, Vector2.Down, Vector2.Left };
		private Vector2 spawnPosition = new Vector2();

		private enum Tiles { EMPTY = 0, GROUND = 1, WALL = 2 };

		public override void _Ready()
		{
			rockLayer = GetNode("../YSort/Rocks");
		}

		public void Generate()
		{
			rng.Randomize();
			Clear();

			for (int x = 0; x < mapSize.x; x++)
			{
				for (int y = 0; y < mapSize.y; y++)
				{
					SetCell(x, y, (int)Tiles.WALL);
				}
			}

			CreateFloors();
			RemoveSingleWalls();
			ChooseSpawnPoint();
			CreateRocks();
		}

		private void CreateFloors()
		{
			int i = 0;
			Vector2 pos = new Vector2(mapSize.x / 2, mapSize.y / 2);
			Walker w = new Walker(pos, GetRandomDirection());
			walkers.Add(w);

			while (i < iterations)
			{
				foreach (Walker walker in walkers.ToList())
				{
					// Sometimes change direction while moving
					if (rng.Randf() < randomDirectionChance)
					{
						walker.currentDirection = GetRandomDirection();
					}

					walker.position += walker.currentDirection;
					walker.position.x = Mathf.Clamp(walker.position.x, 1, (int)mapSize.x - paddingFromWall);
					walker.position.y = Mathf.Clamp(walker.position.y, 1, (int)mapSize.y - paddingFromWall);

					SetCell((int)walker.position.x, (int)walker.position.y, (int)Tiles.GROUND);
					floorTiles.Add(MapToWorld(walker.position));

					// Sometimes add a new walker
					if (rng.Randf() < walkerSpawnChance && walkers.Count < maxWalkers)
					{
						Walker walk = new Walker(walker.position, GetRandomDirection());
						walkers.Add(walk);
					}

					// Sometimes remove a walker
					if (rng.Randf() < walkerSpawnChance / 2 && walkers.Count > 1) walkers.Remove(walker);
				}

				i++;

				if (floorTiles.Count / (mapSize.x * mapSize.y) > percentageToFill)
				{
					break;
				}
			}

			floorTiles = floorTiles.Distinct().ToList();
		}

		// Iterates through the map and removes any single wall tiles
		private void RemoveSingleWalls()
		{
			for (int x = 0; x < mapSize.x - 1; x++)
			{
				for (int y = 0; y < mapSize.y - 1; y++)
				{
					if (GetCell(x, y) == (int)Tiles.WALL)
					{
						bool allFloors = true;

						for (int checkX = -1; checkX <= 1; checkX++)
						{
							for (int checkY = -1; checkY <= 1; checkY++)
							{
								if (x + checkX < 0 || x + checkX > mapSize.x - 1 || y + checkY < 0 || y + checkY > mapSize.y - 1) continue; // skips checks that are out of range
								if ((checkX != 0 && checkY != 0) || (checkX == 0 && checkY == 0)) continue; // skips corners and center

								if (GetCell(x + checkX, y + checkY) != (int)Tiles.GROUND) allFloors = false;
							}
						}

						if (allFloors)
						{
							SetCell(x, y, (int)Tiles.GROUND);
							floorTiles.Add(MapToWorld(new Vector2(x, y)));
						}
					}
				}
			}
		}

		private void ChooseSpawnPoint()
		{
			rng.Randomize();
			Vector2 p = floorTiles[rng.RandiRange(0, floorTiles.Count - 1)];

			floorTiles.RemoveAll(x => x == p);
			spawnPosition = p;
		}

		private void CreateRocks()
		{
			rng.Randomize();
			int cachedCount = floorTiles.Count;

			foreach (Vector2 tileCoord in floorTiles)
			{
				// Spawn a rock
				if (rng.Randf() < 0.2)
				{
					StaticBody2D r = new StaticBody2D();

					// Spawn ore instead
					if (rng.Randf() < 0.2)
					{
						r = (StaticBody2D)basicOre.Instance();
					}
					else
					{
						r = (StaticBody2D)rocks[rng.RandiRange(0, rocks.Length - 1)].Instance();
					}

					rockTiles.Add(tileCoord);
					r.GlobalPosition = tileCoord;
					rockLayer.AddChild(r);
				}
			}

			// Lets other scripts know that the level is done generating
			EmitSignal(nameof(LevelGenerated), spawnPosition);
		}

		private Vector2 GetRandomDirection()
		{
			rng.Randomize();
			return directions[rng.RandiRange(0, 3)];
		}
	}
}
