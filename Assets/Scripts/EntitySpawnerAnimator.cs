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

    private int mTakeIdleInd = -1;
    private int mTakeSpawningInd = -1;
    private int mTakeSpawnInd = -1;

    void Awake() {
        if(animator) {
            mTakeIdleInd = animator.GetTakeIndex(_takeIdle);
            mTakeSpawningInd = animator.GetTakeIndex(_takeSpawning);
            mTakeSpawnInd = animator.GetTakeIndex(_takeSpawn);
        }
    }

    void IEntitySpawnerListener.OnSpawnStart() {
        if(mTakeIdleInd == -1)
            return;

        animator.Play(mTakeIdleInd);
    }

    void IEntitySpawnerListener.OnSpawnReady() {
        if(mTakeSpawningInd == -1)
            return;

        animator.Play(mTakeSpawningInd);
    }

    void IEntitySpawnerListener.OnSpawnBegin() {
        if(mTakeSpawnInd == -1)
            return;

        animator.Play(mTakeSpawnInd);
    }

    bool IEntitySpawnerListener.OnSpawning() {
        if(mTakeSpawnInd == -1)
            return true;

        return animator.currentPlayingTakeIndex != mTakeSpawnInd || !animator.isPlaying;
    }

    void IEntitySpawnerListener.OnSpawnEnd() {
        if(mTakeSpawningInd == -1)
            return;

        animator.Play(mTakeSpawningInd);
    }

    void IEntitySpawnerListener.OnSpawnStop() {
        if(mTakeIdleInd == -1)
            return;

        animator.Play(mTakeIdleInd);
    }

    void IEntitySpawnerListener.OnSpawnFull(bool full) {
        if(full) {
            if(mTakeIdleInd == -1)
                return;

            animator.Play(mTakeIdleInd);
        }
        else {
            if(mTakeSpawningInd == -1)
                return;

            animator.Play(mTakeSpawningInd);
        }
    }
}
