using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

// ReSharper disable CheckNamespace
public class Layout : GameBase
{
    [Flags]
    public enum MapTileType : byte
    {
        Floor = 1,
        WayPoint = 2,
        Monster = 4
    }

    public class MapTile
    {
        public MapTileType Type { get; set; }
        public List<GameObject> Parts { get; private set; }

        public MapTile()
        {
            Parts = new List<GameObject>();
        }

        public void Activate()
        {
            foreach (var wall in Parts)
            {
                wall.GetComponent<Renderer>().enabled = true;
            }
        }

        public void Deactivate()
        {
            foreach (var wall in Parts)
            {
                wall.GetComponent<Renderer>().enabled = false;
            }
        }
    }

    private Random clock;
    public const int Size = 20;

    protected int X { get; set; }
    protected int Y { get; set; }

    public MapTile[] Map { get; private set; }

    public Material WayPointTexture;
    public Material WallTexture;
    public Material FloorTexture;

    // Use this for initialization
    protected override void Start()
    {
        clock = new Random(Guid.NewGuid().GetHashCode());
        Map = new MapTile[Size*Size];

        for (var i = 0; i < Map.Length; i++)
        {
            Map[i] = new MapTile();
        }

        var current = new Point(this, null, null, Direction.None);
        while ((current = current.Render()) != null)
        {
            // nothing
        }

        for (var y = 0; y < Size; y++)
        {
            for (var x = 0; x < Size; x++)
            {
                var tile = Get(x, y);
                if (tile.Type == 0)
                {
                    continue;
                }

                CreateFloor(tile, x, y);
                CreateRoof(tile, x, y);

                if (x - 1 < 0 || Get(x - 1, y).Type == 0)
                {
                    CreateWallObject(tile, x*10 - 5, 5, Size*10 - y*10, Mathf.Deg2Rad*90, Mathf.Deg2Rad*90);
                }
                if (y - 1 < 0 || Get(x, y - 1).Type == 0)
                {
                    CreateWallObject(tile, x*10, 5, Size*10 - y*10 + 5, Mathf.Deg2Rad*90, Mathf.Deg2Rad*180);
                }

                if (x + 1 > Size - 1 || Get(x + 1, y).Type == 0)
                {
                    CreateWallObject(tile, x*10 + 5, 5, Size*10 - y*10, Mathf.Deg2Rad*90, Mathf.Deg2Rad*-90);
                }

                if (y + 1 > Size - 1 || Get(x, y + 1).Type == 0)
                {
                    CreateWallObject(tile, x*10, 5, Size*10 - y*10 - 5, Mathf.Deg2Rad*90, 0);
                }

                if ((tile.Type & MapTileType.WayPoint) == MapTileType.WayPoint)
                {
                    var wp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    wp.name = "WayPoint";
                    wp.GetComponent<Renderer>().material = WayPointTexture;
                    wp.transform.SetPosition(x*10, 2f, Size*10 - y*10);
                    wp.isStatic = true;
                }

                if ((tile.Type & MapTileType.Monster) == MapTileType.Monster)
                {
                    var wp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    wp.name = "Monster";
                    wp.layer = LayerMask.NameToLayer("Monster");
                    //wp.AddComponent<Rigidbody>();
                    wp.GetComponent<Renderer>().material = WayPointTexture;
                    wp.transform.SetPosition(x*10, 2f, Size*10 - y*10);
                    wp.AddComponent<Monster>();
                    wp.AddComponent<SphereCollider>();

                    var sc = wp.GetComponent<SphereCollider>();
                    sc.radius = 0.5f;
                }
            }
        }

        transform.SetPosition(0, 2f, Size*10);
        transform.RotateAround(transform.up,
                               (float) ((Get(1, 0).Type & MapTileType.Floor) == MapTileType.Floor ? Math.PI/2 : Math.PI));

        wallMask = 1 << LayerMask.NameToLayer("Wall");
    }

    private static void CreateRoof(MapTile parent, int x, int y)
    {
        var roof = GameObject.CreatePrimitive(PrimitiveType.Plane);
        roof.transform.SetPosition(x*10, 10, Size*10 - y*10);
        roof.transform.RotateAround(Vector3.forward, Mathf.PI);
        roof.isStatic = true;
        roof.GetComponent<Renderer>().enabled = false;
        parent.Parts.Add(roof);
    }

    private void CreateFloor(MapTile parent, int x, int y)
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.layer = LayerMask.NameToLayer("Floor");
        floor.GetComponent<Renderer>().material = FloorTexture;
        floor.transform.SetPosition(x*10, 0, Size*10 - y*10);
        floor.AddComponent<BoxCollider>();
        var b = floor.GetComponent<BoxCollider>();
        b.center = new Vector3(0, -5, 0);
        b.size = Vector3.one*10f;
        floor.isStatic = true;
        floor.GetComponent<Renderer>().enabled = false;
        parent.Parts.Add(floor);
    }

    private void CreateWallObject(MapTile parent, float x, float y, float z, float right, float up)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Plane);
        wall.layer = LayerMask.NameToLayer("Wall");
        wall.GetComponent<Renderer>().material = WallTexture;
        wall.transform.SetPosition(x, y, z);
        wall.transform.RotateAround(Vector3.right, right);
        wall.transform.RotateAround(Vector3.up, up);
        wall.isStatic = true;

        wall.GetComponent<Renderer>().enabled = false;

        parent.Parts.Add(wall);
        cullingTable[wall] = parent;
    }

    public static Vector2 WorldToGrid(float x, float y)
    {
        return new Vector2(Mathf.RoundToInt(x/10), Mathf.RoundToInt(Size - y/10));
    }

    public static Vector2 WorldToGrid(Vector3 pos)
    {
        return WorldToGrid(pos.x, pos.z);
    }

    public static Vector3 GridToWorld(Vector2 pos)
    {
        return GridToWorld(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
    }

    public static Vector3 GridToWorld(int x, int y)
    {
        return new Vector3(x*10, 0, Size*10 - y*10);
    }

    private int wallMask;
    private const float PortStep = 0.01f;
    private readonly Dictionary<GameObject, MapTile> cullingTable = new Dictionary<GameObject, MapTile>();
    private readonly List<MapTile> activeTiles = new List<MapTile>();
    private int activeTilesCount;

    private void AppendActiveTile(MapTile tile)
    {
        if (activeTiles.Count > activeTilesCount)
        {
            activeTiles[activeTilesCount++] = tile;
        }
        else
        {
            activeTiles.Add(tile);
        }
    }

    private void Travel(Vector2 origin, Vector2 dir)
    {
        while (Get(origin + dir) != null && Get(origin + dir).Type != 0)
        {
            origin += dir;
            var tile = Get(origin);
            tile.Activate();
            AppendActiveTile(tile);

            var next = (Vector2) (Quaternion.Euler(0, 0, 90)*dir);
            if (Get(origin + next) != null && Get(origin + next).Type != 0)
            {
                var nt = Get(origin + next);
                nt.Activate();
                AppendActiveTile(nt);
                Debug.DrawLine(transform.position, GridToWorld(origin + next), Color.red);
            }

            next = Quaternion.Euler(0, 0, -90) * dir;
            if (Get(origin + next) != null && Get(origin + next).Type != 0)
            {
                var nt = Get(origin + next);
                nt.Activate();
                AppendActiveTile(nt);
                Debug.DrawLine(transform.position, GridToWorld(origin + next), Color.red);
            }

        }

        Debug.DrawLine(transform.position, GridToWorld(origin), Color.red);
    }

    // Update is called once per frame
    protected override void Update()
    {
        for (var i = 0; i < activeTilesCount; i++)
        {
            activeTiles[i].Deactivate();
        }
        activeTilesCount = 0;

        var origin = WorldToGrid(transform.position);
        Get(origin).Activate();
        activeTiles.Add(Get(origin));

        Travel(origin, Vector2.up);
        Travel(origin, -Vector2.up);
        Travel(origin, Vector2.right);
        Travel(origin, -Vector2.right);



//
//        foreach (var hit in Physics.SphereCastAll(transform.position, 200, transform.forward, 1, wallMask))
//        {
//            var dir = (hit.point - transform.position).normalized;
//            if (Vector3.Angle(transform.forward, dir) > 180f)
//            {
//                Debug.Log(Vector3.Angle(transform.forward, dir));
//                continue;
//            }
//
//            var tile = cullingTable[hit.transform.gameObject];
//            tile.Activate();
//
//            activeTiles.Add(tile);
//        }

//        for (var x = 0f; x <= 1f; x += PortStep)
//        {
//            var ray = Camera.main.ViewportPointToRay(new Vector3(x, 0.5f, 0));
//            RaycastHit hit;
//            if (Physics.Raycast(ray, out hit, Mathf.Infinity, wallMask))
//            {
//                var tile = cullingTable[hit.transform.gameObject];
//                tile.Activate();
//
//                activeTiles[index++] = tile;
//            }
//
//            
//        }
    }

    private static bool Inbound(int coord)
    {
        return coord >= 0 && coord < Size;
    }

    public MapTile Get(Vector2 coord)
    {
        return Get(Mathf.RoundToInt(coord.x), Mathf.RoundToInt(coord.y));
    }

    public MapTile Get(int x, int y)
    {
        if (!Inbound(x) || !Inbound(y))
        {
            return null;
        }

        var index = y*Size + x;
        return Map[index];
    }

    public void Set(int x, int y, MapTileType n, bool flag = false)
    {
        if (!Inbound(x) || !Inbound(y))
        {
            return;
        }
        var index = Get(x, y);
        index.Type = flag ? index.Type | n : n;
    }

    protected bool Valid(int x, int y)
    {
        if (x < 0 || x >= Size || y < 0 || y >= Size || Get(x, y).Type != 0)
        {
            return false;
        }

        var i = 0;

        if (x > 0)
        {
            i += (Get(x - 1, y).Type & MapTileType.Floor) == MapTileType.Floor ? 1 : 0;
        }
        if (x < Size - 1)
        {
            i += (Get(x + 1, y).Type & MapTileType.Floor) == MapTileType.Floor ? 1 : 0;
        }
        if (y > 0)
        {
            i += (Get(x, y - 1).Type & MapTileType.Floor) == MapTileType.Floor ? 1 : 0;
        }
        if (y < Size - 1)
        {
            i += (Get(x, y + 1).Type & MapTileType.Floor) == MapTileType.Floor ? 1 : 0;
        }

        return i <= 1;
    }

    private enum Direction
    {
        North,
        East,
        South,
        West,
        None
    }

    private class Point
    {
        private Point next;

        private readonly Layout layout;
        private readonly Point home;
        private readonly Point parent;
        private readonly Direction d;

        public Point(Layout layout, Point home, Point parent, Direction d)
        {
            this.layout = layout;
            this.home = home ?? this;
            this.parent = parent;
            this.d = d;
        }

        public Point Render()
        {
            layout.Set(layout.X, layout.Y, MapTileType.Floor);

            do
            {
                switch (layout.clock.Next(4))
                {
                    case 0:
                        if (layout.X > 0 && layout.Valid(layout.X - 1, layout.Y))
                        {
                            layout.X--;
                            return next = new Point(layout, home, this, Direction.West);
                        }
                        break;
                    case 1:
                        if (layout.X < Size - 1 && layout.Valid(layout.X + 1, layout.Y))
                        {
                            layout.X++;
                            return next = new Point(layout, home, this, Direction.East);
                        }
                        break;
                    case 2:
                        if (layout.Y > 0 && layout.Valid(layout.X, layout.Y - 1))
                        {
                            layout.Y--;
                            return next = new Point(layout, home, this, Direction.North);
                        }
                        break;
                    case 3:
                        if (layout.Y < Size - 1 && layout.Valid(layout.X, layout.Y + 1))
                        {
                            layout.Y++;
                            return next = new Point(layout, home, this, Direction.South);
                        }
                        break;
                }
            } while (layout.Valid(layout.X - 1, layout.Y) || layout.Valid(layout.X + 1, layout.Y) ||
                     layout.Valid(layout.X, layout.Y - 1)
                     || layout.Valid(layout.X, layout.Y + 1));

            if (IsFull())
            {
                return null;
            }

            switch (d)
            {
                case Direction.West:
                    layout.X++;
                    break;
                case Direction.East:
                    layout.X--;
                    break;
                case Direction.North:
                    layout.Y++;
                    break;
                case Direction.South:
                    layout.Y--;
                    break;
            }

            if (parent == null)
            {
                while (true)
                {
                    var wx = layout.clock.Next(Size/2, Size);
                    var wy = layout.clock.Next(Size/2, Size);

                    if (layout.Get(wx, wy).Type == 0)
                    {
                        continue;
                    }

                    layout.Set(wx, wy, MapTileType.WayPoint, true);

                    break;
                }

                for (var i = 0; i < 4; i++)
                {
                    while (true)
                    {
                        var wx = layout.clock.Next(Size/2, Size);
                        var wy = layout.clock.Next(Size/2, Size);

                        if (layout.Get(wx, wy).Type == 0 ||
                            (layout.Get(wx, wy).Type & MapTileType.Monster) == MapTileType.Monster)
                        {
                            continue;
                        }

                        layout.Set(wx, wy, MapTileType.Monster, true);

                        break;
                    }
                }
            }

            return parent;
        }

        private bool IsFull()
        {
            var full = true;
            for (var y = 0; y < Size; y++)
            {
                for (var x = 0; x < Size; x++)
                {
                    if (layout.Get(x, y).Type == 0)
                        full = false;
                }
            }
            return full;
        }
    }
}