using Roguelike;
using System;
using System.Collections;
using UnityEngine;

public class Trap : TileTerrain
{
    public int Damage = 10;
    public bool Invisible = false;
    public SpriteRenderer Sprite;
    public bool IsActive = false;
    public float ProbOfBeingSeen = .5f;

    public void Awake()
    {
        Sprite = GetComponent<SpriteRenderer>();
    }

    public void MakeVisible()
    {
        if (Invisible)
        {
            IsActive = true;
            Invisible = false;
            Sprite.enabled = true;
            var curCol = Sprite.color;
            Sprite.color = new Color(curCol.r, curCol.g, curCol.b, 0);
            StartCoroutine(FadeIn(1f));
        }
    }

    private IEnumerator FadeIn(float timeToFade)
    {
        var time = 0f;
        while (time < 1)
        {
            time += Time.deltaTime / timeToFade;
            var curColor = Sprite.color;
            Sprite.color = new Color(curColor.r, curColor.g, curColor.b, Math.Min(time, 1));
            yield return null;
        }
        var curCol = Sprite.color;
        Sprite.color = new Color(curCol.r, curCol.g, curCol.b, 1);
        IsActive = false;
    }

    public void MakeInvisible()
    {
        Invisible = true;
        Sprite.enabled = false;
    }

    public override float GetPathfindingCost()
    {
        return 1f;  //Assuming 1f is able to be walked through
    }

    public override bool GetPlayerWalkable()
    {
        return true;
    }

    public override Tile.TileTerrainType GetTerrainType()
    {
        return Tile.TileTerrainType.trap;
    }

    public virtual void Activate()
    {
        BattleGrid.instance.StrikeTile(new UnityEngine.Vector2Int(xPos, zPos), new UnityEngine.Vector2Int(xPos, zPos), Damage);
    }
}

