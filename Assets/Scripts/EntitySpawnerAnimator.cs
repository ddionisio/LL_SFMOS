using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Add this to where the entity spawner and mate animator is
/// </summary>
public class EntitySpawnerAnimator : MonoBehaviour, IEntitySpawnerListener {
    public M8.Animator.AnimatorData animator;

    [Header("Takes")]
    [SerializeField]
    string _takeIdle;
    [SerializeField]
    string _takeSpawning;
    [SerializeField]
    string _takeSpawn;

    private int mTakeIdleInd;
    private int mTakeSpawningInd;
    private int mTakeSpawnInd;

    void Awake() {
        mTakeIdleInd = animator.GetTakeIndex(_takeIdle);
        mTakeSpawningInd = animator.GetTakeIndex(_takeSpawning);
        mTakeSpawnInd = animator.GetTakeIndex(_takeSpawn);
    }

    void IEntitySpawnerListener.OnSpawnStart() {
        animator.Play(mTakeSpawningInd);
    }

    void IEntitySpawnerListener.OnSpawnBegin() {
        animator.Play(mTakeSpawnInd);
    }

    bool IEntitySpawnerListener.OnSpawning() {
        return animator.currentPlayingTakeIndex != mTakeSpawnInd || !animator.isPlaying;
    }

    void IEntitySpawnerListener.OnSpawnEnd() {
        animator.Play(mTakeSpawningInd);
    }

    void IEntitySpawnerListener.OnSpawnStop() {
        animator.Play(mTakeIdleInd);
    }
}
