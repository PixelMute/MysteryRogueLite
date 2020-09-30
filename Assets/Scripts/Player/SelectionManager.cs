using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/*
// Deals with the player's clicks and selections to be able to select tiles.
public class SelectionManager : MonoBehaviour
{

    public GameObject tileSelectionPrefab;
    public GameObject spawnedTileSelectionPrefab;
    public bool tileCurrentlySelected = false;
    public Vector3 selectedTile;

    // Stuff to detect wether the player is clicking on the UI
    GraphicRaycaster raycaster;
    PointerEventData pointerEventData;
    EventSystem eventSystem;

    private void Start()
    {
        spawnedTileSelectionPrefab = Instantiate(tileSelectionPrefab);
        spawnedTileSelectionPrefab.SetActive(false);

        raycaster = FindObjectOfType<GraphicRaycaster>();
        eventSystem = FindObjectOfType<EventSystem>();
    }

    private void Update()
    {
        // Has the player clicked?
        if (Input.GetMouseButtonDown(0))
        {
            // Detect wether this click is on the UI
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            raycaster.Raycast(pointerEventData, results);

            if (results.Count > 0)
            {
                HandleUIClick(results);
            }
            else
            {
                HandleWorldClick();
            }
        }
    }

    // This handles a click that doesn't hit any UI elements.
    // Currently, this just handles selecting a world tile.
    private void HandleWorldClick()
    {
        // We want to cast a line from the camera onto the ground plane and see what it collides with in order to see which tile the player clicks.
        Plane playerPlane = new Plane(Vector3.up, PlayerController.instance.transform.position);
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;

        if (playerPlane.Raycast(ray, out float hitDistance))
        {
            Vector3 mousePosition = ray.GetPoint(hitDistance);
            // Want to spawn tileSelectionPrefab around here.

            // Need logic for if x or z is negative.
            float spawnX;
            float spawnZ;
            if (mousePosition.x >= 0)
                spawnX = (int)mousePosition.x + 0.5f;
            else
                spawnX = (int)mousePosition.x - 0.5f;

            if (mousePosition.z >= 0)
                spawnZ = (int)mousePosition.z + 0.5f;
            else
                spawnZ = (int)mousePosition.z - 0.5f;

            Vector3 newSpawnLocation = new Vector3(spawnX + GameManager.offset, PlayerController.instance.transform.position.y, spawnZ + GameManager.offset);
            // Is this already selected?
            if (tileCurrentlySelected && spawnedTileSelectionPrefab.transform.position.Equals(newSpawnLocation))
                DeselectTile();
            else
                SelectTile(newSpawnLocation);
            
        }
    }

    public void DeselectTile()
    {
        tileCurrentlySelected = false;
        spawnedTileSelectionPrefab.SetActive(false);
    }

    public void SelectTile(Vector3 target)
    {
        tileCurrentlySelected = true;
        spawnedTileSelectionPrefab.SetActive(true);
        spawnedTileSelectionPrefab.transform.position = target;
        selectedTile = target;

        // Now we want to call the PlayerController.
        // If this uses a card, deselect the tile.
        if (PlayerController.instance.TileSelected(target))
        {
            DeselectTile();
        }
    }

    private void HandleUIClick(List<RaycastResult> results)
    {
        DeselectTile();
        foreach (RaycastResult x in results)
        {
            Debug.Log("Clicked " + x);
            CardInterface clickedCard = x.gameObject.transform.parent.GetComponent<CardInterface>();
            Debug.Log("Calling click on card");
            if (clickedCard != null) { }
                //clickedCard.CardClicked();
        }
    }
}
*/