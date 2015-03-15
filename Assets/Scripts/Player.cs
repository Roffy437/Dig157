﻿using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    #region Attributs publics
    public float speed = 10;
    public float cameraSpeed = 3;
    public float cameraHeight = 10;
	public float cameraDist = 8;
    public GameObject dynamitePrefab;
    public LayerMask layerMaskBlock;
    public LayerMask oreMaskBlock;
    #endregion

    #region Attributs privés
    private Vector3 targetPos;
    private bool isMovable = true;
	private Quaternion dynamiteRotation = Quaternion.identity;
    #endregion

    #region Méthodes privées
    void Move () {
        targetPos += new Vector3 (TouchManager.Instance.SwipeAxis.x, 0, TouchManager.Instance.SwipeAxis.y);
        
        // Rotation
        float y = TouchManager.Instance.SwipeAxis.x * 90;
        if (-1 == TouchManager.Instance.SwipeAxis.y) {
            y = 180;
        }
        transform.rotation = Quaternion.Euler(new Vector3 (0, y, 0));
        
        // Minerai catch
        RaycastHit hit;
        // Forward
        if (Physics.Raycast (targetPos, transform.forward, out hit, 1, oreMaskBlock)) {
            hit.collider.GetComponent<Ore> ().Target = transform;
        }
        // Left
        if (Physics.Raycast (targetPos, -transform.right, out hit, 1, oreMaskBlock)) {
            hit.collider.GetComponent<Ore> ().Target = transform;
        }
        // Right
        if (Physics.Raycast (targetPos, transform.right, out hit, 1, oreMaskBlock)) {
            hit.collider.GetComponent<Ore> ().Target = transform;
        }

        isMovable = false;
        BlocksManager.Instance.UpdateBlocks (targetPos, TouchManager.Instance.SwipeAxis);
    }

    void Start()
    {
        targetPos = transform.position;
    }
    void ThrowDynamite () {
		dynamiteRotation.eulerAngles = new Vector3(0, Random.Range(0,360), 0);
        GameObject dynamiteObject = Instantiate (dynamitePrefab, transform.position - Vector3.up / 2, dynamiteRotation) as GameObject;
    }

    void Update() {
        RaycastHit hit;
        if (isMovable && TouchManager.Instance.CurrentGesture != TouchManager.Gestures.None && TouchManager.Instance.CurrentGesture != TouchManager.Gestures.DoubleTap) {
            if (Physics.Raycast (targetPos, new Vector3 (TouchManager.Instance.SwipeAxis.x, 0, TouchManager.Instance.SwipeAxis.y), out hit, 1, layerMaskBlock))
            {
                if ("Empty Block" == hit.collider.tag) {
                    hit.collider.gameObject.GetComponent<Block> ().Die ();
                    ThrowDynamite ();
                    Move ();
                    transform.GetChild (0).GetComponent<Animation> ().Stop ();
                    transform.GetChild (0).GetComponent<Animation> ().Play ("Blast");
                }
            }
            else {
                ThrowDynamite ();
                Move ();
                transform.GetChild (0).GetComponent<Animation> ().Play ("Walk");
            }
        }
        transform.position = Vector3.Lerp (transform.position, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance (transform.position, targetPos) < 0.05f) {
            transform.position = targetPos; // Clamp

            if (0 == Random.Range (0, 10)) {
                transform.GetChild (0).GetComponent<Animation> ().PlayQueued ("Look");
            }
            else {
                transform.GetChild (0).GetComponent<Animation> ().PlayQueued ("Idle");
            }
        
            isMovable = true;
        }

        Camera.main.transform.position = Vector3.Lerp (Camera.main.transform.position, new Vector3(transform.position.x, cameraHeight, transform.position.z - cameraDist), cameraSpeed * Time.deltaTime);
    }
    #endregion
}
