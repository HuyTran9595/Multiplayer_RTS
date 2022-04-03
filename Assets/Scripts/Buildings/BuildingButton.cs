using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Assets.Scripts.Buildings
{
    public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Building building = null;
        [SerializeField] Image iconImage = null;
        [SerializeField] TMP_Text priceText = null;
        [SerializeField] LayerMask floorMask = new LayerMask();


        Camera mainCamera;
        RTSPlayer player;
        GameObject buildingPreviewInstance;
        Renderer buildingRendererInstance;


        private void Start()
        {
            mainCamera = Camera.main;
            iconImage.sprite = building.GetIcon();
            priceText.text = building.GetPrice().ToString();
           
        }

        private void Update()
        {
            if(player == null)
            {
                player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
            }

            if(buildingPreviewInstance is null) {
                return;
            }
            UpdateBuildingPreview();
        }



        //generate building preview on screen
        public void OnPointerDown(PointerEventData eventData)
        {
            if(eventData.button != PointerEventData.InputButton.Left) { 
                return;
            }
            buildingPreviewInstance = Instantiate(building.GetBuildingPreview());
            buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();
            buildingPreviewInstance.SetActive(false);

        }

        //place building if player release mouse button and the mouse is pointing towards correct layers
        public void OnPointerUp(PointerEventData eventData)
        {
            if(buildingPreviewInstance is null) {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) {
                //place building
                player.CmdTryPlaceBuilding(building.GetId(), hit.point);
            }

            Destroy(buildingPreviewInstance);
        }


        //render the building preview on screen whenever mouse hit the correct layers.
        private void UpdateBuildingPreview() {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) {
                return;
            }
            if(buildingPreviewInstance != null) {
                buildingPreviewInstance.transform.position = hit.point;
                if (!buildingPreviewInstance.activeSelf) {
                    buildingPreviewInstance.SetActive(true);
                }
            }
            

        }
    }
}