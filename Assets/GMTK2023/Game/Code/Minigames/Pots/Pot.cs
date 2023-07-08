using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GMTK2023.Game.MiniGames {

	public class Pot : MonoBehaviour {

#region Events

		public Action? PotCleaned;

#endregion

#region Fields

		[SerializeField] private SpriteRenderer? spriteRenderer;
		[SerializeField] private GameObject? potPiecePrefab;
		[SerializeField] private float pieceRadius;

#endregion

#region Properties

		public bool IsFilledWithCoins { get; set; }
		public int BrokenPiecesCount { get; set; }

		private IList<PotPiece> BrokenPieces { get; set; } = new Collection<PotPiece>();

#endregion

#region Methods

		public void SetupPot() {
			spriteRenderer!.enabled = true;
		}

		public void Smash() {

			BrokenPiecesCount = Random.Range(3, 6);

			spriteRenderer!.enabled = false;

			for (int i = 0; i < BrokenPiecesCount; i++) {
				Vector2 newPos = new Vector2(
						transform.position.x + Random.Range(-pieceRadius, pieceRadius),
						transform.position.y + Random.Range(-pieceRadius, pieceRadius)
					);

				PotPiece fallenPiece = Instantiate(potPiecePrefab, newPos, Quaternion.identity, transform)!.GetComponent<PotPiece>();
				fallenPiece.PieceBroomed += CleanedPiece;

				BrokenPieces.Add(fallenPiece);
			}

		}

		public void CleanedPiece() {
			BrokenPiecesCount--;

			if (BrokenPiecesCount == 0) {
				PotCleaned?.Invoke();
			}
		}

#endregion

	}

}