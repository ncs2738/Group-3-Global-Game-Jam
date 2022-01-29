using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Tile : MonoBehaviour
{
    [SerializeField]
    private GameObject hoverHighlight;
    [SerializeField]
    private GameObject movementHighlight;

    [SerializeField]
    private GameObject blueAttackHighlight;
    [SerializeField]
    private GameObject redAttackHighlight;

    [SerializeField]
    protected TileType tileType;
    [SerializeField]
    protected bool isWalkable = true;

    [SerializeField]
    protected ConcreteUnit occupiedUnit = null;

    private TileType maxTypeVal;
    private TileType minTypeVal;

    private bool isHovered = false;

    public enum TileType
    {
        Grass = 0,
        Fortress = 1,
        PlayerCastle = 2,
        SpawnableTile = 3,
    }

    private void Start()
    {
        maxTypeVal = System.Enum.GetValues(typeof(TileType)).Cast<TileType>().Max();
        minTypeVal = System.Enum.GetValues(typeof(TileType)).Cast<TileType>().Min();
    }

    protected void OnMouseEnter()
    {
        //First, check if there is a unit on the tile
        if(occupiedUnit != null)
        {
            //Then check to make sure we've not hovered over unit currently selected...
            if(!GridManager.Instance.IsUnitSelected(occupiedUnit))
            {
                isHovered = true;
                //occupiedUnit.GetAvailableMoves(Unit.UnitMoveTypes.Dragon);
                //occupiedUnit.ShowAvailableMoves(true);
            }
        }

        SetHoverHighlight(true);
    }

    protected void OnMouseExit()
    {
        if(isHovered)
        {
            isHovered = false;
            //occupiedUnit.ShowAvailableMoves(false);
        }

        if(!GridManager.Instance.IsTileInMovePool(this))
        {
            SetHoverHighlight(false);
        }
    }

    public void SetHoverHighlight(bool status)
    {
        hoverHighlight.SetActive(status);
    }

    protected void OnMouseOver()
    {
        if (GridManager.Instance.IsMapEditEnabled())
        {
            EditModeInputs();
        }
        else
        {
            if(Input.GetMouseButtonDown(0))
            {
                GridManager.Instance.LeftClickInputHandler(this, occupiedUnit);
            }

            GameModeInputs();
        }
    }

    private void EditModeInputs()
    {
        void AddUnit()
        {
            if (occupiedUnit == null)
            {
                UnitManager.Instance.AddUnit(this);
            }
            else
            {
                Debug.Log(occupiedUnit);
                UnitManager.Instance.RemoveUnit(occupiedUnit);
            }
        }

        void IncrementTileType()
        {
            tileType++;

            /*
            I'm really not sure why this isn't working... for some reason the maxTypeVal keeps changing? it's quite odd. I'm just gonna hardcode it.
            if (tileType > maxTypeVal)
            */
            if ((int) tileType > 3)
            {
                tileType = minTypeVal;
            }

            GridManager.Instance.SetTileType(this, tileType);
        }

        void DecrementTileType()
        {
            tileType--;

            if (tileType < minTypeVal)
            {
                tileType = maxTypeVal;
            }

            GridManager.Instance.SetTileType(this, tileType);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.U) && UnitManager.Instance.HasSelectedUnit())
            {
                AddUnit();
            }
            else
            {
                IncrementTileType();
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            DecrementTileType();
        }

        if (Input.GetMouseButtonDown(2) && UnitManager.Instance.HasSelectedUnit())
        {
            AddUnit();
        }

        if (occupiedUnit)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                UnitManager.Instance.SwapUnitTeam(occupiedUnit);
            }
        }
    }

    protected virtual void GameModeInputs()
    {
        //UnitManager.Instance.CastSpell(tile: this, card: null);
        return;
    }

    public bool IsTileEmpty()
    {
        if(isWalkable && occupiedUnit == null)
        {
            return true;
        }

        return false;
    }

    public bool IsPassable(PlayerTeam.Faction faction)
    {
        if (isWalkable)
        {
            if((occupiedUnit == null || occupiedUnit.faction.Equals(faction)))
            {
                return true;
            }
            return false;
        }

        return false;
    }

    public void OccupyTile(ConcreteUnit newUnit)
    {
        occupiedUnit = newUnit;
    }

    public void RemoveUnit()
    {
        occupiedUnit = null;
    }

    public TileType GetTileType()
    {
        return tileType;
    }

    [System.Serializable]
    public class SaveObject
    {
        public TileType tileType;
        public bool isWalkable;
        public float posX;
        public float posY;
        public ConcreteUnit.SaveObject occupiedUnit;
        public SpawnableTile.SaveObject spawnableTileData;
    }

    public SaveObject Save()
    {
        SpawnableTile spawnTile = this as SpawnableTile;
        return new SaveObject
        {
            tileType = tileType,
            isWalkable = isWalkable,
            posX = transform.position.x,
            posY = transform.position.y,
            occupiedUnit = occupiedUnit != null ? occupiedUnit.Save() : null,
            spawnableTileData = spawnTile != null ? spawnTile.Save() : null,
        };
}

    public void Load(ConcreteUnit.SaveObject loadedUnit, SpawnableTile.SaveObject spawnableTileData = null)
    {
        UnitManager.Instance.LoadUnit(this, loadedUnit);

        SpawnableTile spawnTile = this as SpawnableTile;
        if (spawnTile != null && spawnableTileData != null)
        {
            //spawnTile.SetTileOwner(spawnableTileData.tileOwner);
        }
    }
}
