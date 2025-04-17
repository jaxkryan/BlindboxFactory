using UnityEngine;
using UnityEngine.Tilemaps;

namespace BuildingSystem {
    [RequireComponent(typeof(Tilemap))]
    public class TilemapLayer : PersistentSingleton<TilemapLayer> {
        protected Tilemap _tilemap { get; private set; }

        protected override void Awake() {
            base.Awake();
            _tilemap = GetComponent<Tilemap>();
        }
    }
}