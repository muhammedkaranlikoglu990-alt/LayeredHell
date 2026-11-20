// 45: Layered Hell - 4-Layer Zombie AI (L1 Sight/Hearing, L2 FSM, L3 Archetype Specials, L4 Pack Alert).
using UnityEngine;
using UnityEngine.AI;

namespace LayeredHell
{
    public enum ZombieState
    {
        Idle,
        Wander,
        Investigate,
        Chase,
        Attack,
        Stagger,
        Dead
    }

    public class ZombieAI : MonoBehaviour, IDamageable
    {
        public string archetypeId = "walker";
        public int floor;
        public int spawnIndex = -1;

        public ZombieArchetype Archetype { get; private set; }
        public ZombieState State { get; private set; } = ZombieState.Idle;
        public float Health { get; private set; }
        public float MaxHealth { get; private set; }
        public bool IsDead { get; private set; }

        public Animator Animator { get; set; }
        public NavMeshAgent Agent { get; set; }

        private Vector3 _homePosition;
        private Vector3 _investigateTarget;
        private float _stateTimer;
        private float _attackTimer;
        private float _perceptionTimer;
        private float _vocalTimer;
        private float _speedMultiplier = 1f;
        private string _currentClip = "";
        private bool _attackHitDelivered;

        public void Initialize(string archId, int floorIndex, int spIndex = -1)
        {
            archetypeId = string.IsNullOrEmpty(archId) ? "walker" : archId.ToLowerInvariant();
            floor = floorIndex;
            spawnIndex = spIndex;

            Archetype = ZombieArchetype.Get(archetypeId);
            MaxHealth = Archetype.hp;
            Health = MaxHealth;
            IsDead = false;

            _homePosition = transform.position;
            _investigateTarget = _homePosition;
            _vocalTimer = Random.Range(2.5f, 8.0f);
            _perceptionTimer = Random.Range(0.05f, 0.25f);

            if (Agent == null) Agent = GetComponent<NavMeshAgent>();
            if (Animator == null) Animator = GetComponentInChildren<Animator>();

            if (Agent != null)
            {
                Agent.speed = Archetype.speed;
                Agent.angularSpeed = 320f;
                Agent.acceleration = 14f;
                Agent.stoppingDistance = Mathf.Min(1.35f, Archetype.attackRange * 0.75f);
            }

            EnterState(ZombieState.Idle);
        }

        private void OnEnable()
        {
            NoiseSystem.NoiseEmitted += OnNoiseEmitted;
        }

        private void OnDisable()
        {
            NoiseSystem.NoiseEmitted -= OnNoiseEmitted;
        }

        private void Start()
        {
            if (Archetype == null)
            {
                Initialize(archetypeId, floor, spawnIndex);
            }
        }

        private void Update()
        {
            if (IsDead) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            float dt = Time.deltaTime;
            _stateTimer -= dt;
            _attackTimer -= dt;
            _perceptionTimer -= dt;
            _vocalTimer -= dt;

            if (_vocalTimer <= 0f)
            {
                _vocalTimer = Random.Range(5f, 12f);
                PlayArchetypeSound("breath", 0.55f);
            }

            // L1 Perception check (throttled to ~6 Hz)
            if (_perceptionTimer <= 0f)
            {
                _perceptionTimer = 0.16f;
                UpdatePerception();
            }

            // L2 FSM execution
            switch (State)
            {
                case ZombieState.Idle:
                    TickIdle();
                    break;
                case ZombieState.Wander:
                    TickWander(dt);
                    break;
                case ZombieState.Investigate:
                    TickInvestigate(dt);
                    break;
                case ZombieState.Chase:
                    TickChase(dt);
                    break;
                case ZombieState.Attack:
                    TickAttack(dt);
                    break;
                case ZombieState.Stagger:
                    TickStagger();
                    break;
            }
        }

        public void SetSpeedMultiplier(float mult)
        {
            _speedMultiplier = Mathf.Max(0.4f, mult);
        }

        // L4 Pack Alert entry point
        public void AlertToPosition(Vector3 targetPos, bool immediateChase)
        {
            if (IsDead) return;
            _investigateTarget = targetPos;

            if (immediateChase && State != ZombieState.Attack && State != ZombieState.Stagger)
            {
                EnterState(ZombieState.Chase);
            }
            else if (State == ZombieState.Idle || State == ZombieState.Wander)
            {
                EnterState(ZombieState.Investigate);
            }
        }

        public void ForceHordeAggro()
        {
            if (IsDead) return;
            if (PlayerController.Instance != null)
            {
                _investigateTarget = PlayerController.Instance.Position;
                EnterState(ZombieState.Chase);
            }
        }

        public void TakeDamage(DamageInfo info)
        {
            if (IsDead) return;

            float dmg = Mathf.Max(1f, info.amount);

            // L3 Armored special: ballistic vest reduces body damage, but headshots pierce & deal bonus
            if (info.headshot)
            {
                dmg *= (archetypeId == "armored" ? 2.2f : 1.75f);
                PlayArchetypeSound("bone", 0.9f);
            }
            else if (Archetype != null && Archetype.armor > 0f && info.type == DamageType.Bullet)
            {
                dmg *= (1f - Archetype.armor);
            }

            Health -= dmg;

            if (Health <= 0f)
            {
                Health = 0f;
                Die(info);
                return;
            }

            // Alert self & nearby pack on taking damage
            if (PlayerController.Instance != null)
            {
                _investigateTarget = PlayerController.Instance.Position;
            }

            if (ZombieDirector.Instance != null)
            {
                ZombieDirector.Instance.AlertNearby(transform.position, floor, 12f, _investigateTarget);
            }

            bool shouldStagger = info.headshot || dmg >= MaxHealth * 0.28f || info.type == DamageType.Explosive;
            if (shouldStagger && archetypeId != "goliath")
            {
                EnterState(ZombieState.Stagger);
            }
            else if (State != ZombieState.Chase && State != ZombieState.Attack)
            {
                EnterState(ZombieState.Chase);
            }
        }

        private void UpdatePerception()
        {
            if (PlayerController.Instance == null) return;
            if (PlayerController.Instance.Stats != null && PlayerController.Instance.Stats.IsDead) return;

            Vector3 playerPos = PlayerController.Instance.Position;
            if (Mathf.Abs(playerPos.y - transform.position.y) > GameConstants.FloorHeight * 0.85f)
            {
                return;
            }

            Vector3 eyePos = transform.position + Vector3.up * 1.55f;
            Vector3 targetEye = playerPos + Vector3.up * 1.25f;
            Vector3 toPlayer = targetEye - eyePos;
            float dist = toPlayer.magnitude;

            float effectiveSight = Archetype != null ? Archetype.sightRange : 14f;
            if (PlayerController.Instance.IsCrouching) effectiveSight *= 0.65f;

            if (dist <= effectiveSight)
            {
                float angle = Vector3.Angle(transform.forward, new Vector3(toPlayer.x, 0f, toPlayer.z));
                float halfFov = (Archetype != null ? Archetype.fov : 110f) * 0.5f;

                if (dist < 2.4f || angle <= halfFov)
                {
                    // Check line of sight occlusion
                    bool blocked = false;
                    if (Physics.Raycast(eyePos, toPlayer.normalized, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
                    {
                        if (!hit.transform.IsChildOf(PlayerController.Instance.transform) &&
                            !hit.transform.IsChildOf(transform))
                        {
                            blocked = true;
                        }
                    }

                    if (!blocked)
                    {
                        _investigateTarget = playerPos;
                        if (State != ZombieState.Chase && State != ZombieState.Attack && State != ZombieState.Stagger)
                        {
                            PlayArchetypeSound(archetypeId == "goliath" ? "roar_far" : "scream_far", 0.9f);
                            PlayAnim("Scream");
                            EnterState(ZombieState.Chase);

                            if (ZombieDirector.Instance != null)
                            {
                                ZombieDirector.Instance.AlertNearby(transform.position, floor, 14f, playerPos);
                            }
                        }
                    }
                }
            }
        }

        private void OnNoiseEmitted(Vector3 noisePos, float loudness, GameObject source)
        {
            if (IsDead || Archetype == null) return;
            if (source != null && source == gameObject) return;

            // Vertical floor attenuation
            float verticalDelta = Mathf.Abs(noisePos.y - transform.position.y);
            if (verticalDelta > GameConstants.FloorHeight * 1.25f) return;

            float hearingRadius = loudness * Archetype.hearingMultiplier;
            float dist = Vector3.Distance(transform.position, noisePos);
            if (dist > hearingRadius) return;

            _investigateTarget = noisePos;

            if (loudness >= 35f || dist <= hearingRadius * 0.45f)
            {
                if (State != ZombieState.Chase && State != ZombieState.Attack && State != ZombieState.Stagger)
                {
                    PlayArchetypeSound(archetypeId == "goliath" ? "roar_far" : "scream_far", 0.8f);
                    EnterState(ZombieState.Chase);
                }
            }
            else if (State == ZombieState.Idle || State == ZombieState.Wander)
            {
                EnterState(ZombieState.Investigate);
            }
        }

        private void EnterState(ZombieState next)
        {
            State = next;
            switch (next)
            {
                case ZombieState.Idle:
                    _stateTimer = Random.Range(2.0f, 4.5f);
                    StopMovement();
                    PlayAnim("Idle");
                    break;

                case ZombieState.Wander:
                    _stateTimer = Random.Range(4.0f, 7.5f);
                    Vector2 rnd = Random.insideUnitCircle * 6.5f;
                    _investigateTarget = new Vector3(
                        Mathf.Clamp(_homePosition.x + rnd.x, -21f, 21f),
                        transform.position.y,
                        Mathf.Clamp(_homePosition.z + rnd.y, -21f, 21f));
                    PlayAnim("Walk");
                    break;

                case ZombieState.Investigate:
                    _stateTimer = 6.5f;
                    PlayAnim("Walk");
                    break;

                case ZombieState.Chase:
                    _stateTimer = 12f;
                    bool useRun = archetypeId == "runner" || archetypeId == "goliath" || (Archetype != null && Archetype.speed >= 3.2f);
                    PlayAnim(useRun ? "Run" : "Walk");
                    break;

                case ZombieState.Attack:
                    _stateTimer = 0.95f;
                    _attackHitDelivered = false;
                    StopMovement();
                    PlayAnim(Random.value < 0.5f ? "Attack" : "Attack2");
                    PlayArchetypeSound("attack", 0.95f);
                    break;

                case ZombieState.Stagger:
                    _stateTimer = 0.65f;
                    StopMovement();
                    PlayAnim("Hit");
                    break;

                case ZombieState.Dead:
                    StopMovement();
                    PlayAnim("Death");
                    break;
            }
        }

        private void TickIdle()
        {
            if (_stateTimer <= 0f)
            {
                EnterState(ZombieState.Wander);
            }
        }

        private void TickWander(float dt)
        {
            float spd = (Archetype != null ? Archetype.speed : 2.2f) * 0.55f * _speedMultiplier;
            MoveTowards(_investigateTarget, spd, dt);

            if (_stateTimer <= 0f || HorizontalDistance(transform.position, _investigateTarget) < 1.1f)
            {
                EnterState(ZombieState.Idle);
            }
        }

        private void TickInvestigate(float dt)
        {
            float spd = (Archetype != null ? Archetype.speed : 2.4f) * 0.78f * _speedMultiplier;
            MoveTowards(_investigateTarget, spd, dt);

            if (_stateTimer <= 0f || HorizontalDistance(transform.position, _investigateTarget) < 1.2f)
            {
                EnterState(ZombieState.Idle);
            }
        }

        private void TickChase(float dt)
        {
            if (PlayerController.Instance == null)
            {
                EnterState(ZombieState.Idle);
                return;
            }

            Vector3 playerPos = PlayerController.Instance.Position;
            if (Mathf.Abs(playerPos.y - transform.position.y) > GameConstants.FloorHeight * 0.9f)
            {
                EnterState(ZombieState.Investigate);
                return;
            }

            _investigateTarget = playerPos;
            float dist = HorizontalDistance(transform.position, playerPos);
            float atkRange = Archetype != null ? Archetype.attackRange : 1.75f;

            // Spitter prefers keeping medium distance (3.5..attackRange) when line of sight is clear
            if (archetypeId == "spitter" && dist <= atkRange && dist >= 2.8f)
            {
                FaceTarget(playerPos, dt);
                if (_attackTimer <= 0f)
                {
                    EnterState(ZombieState.Attack);
                }
                else
                {
                    StopMovement();
                }
                return;
            }

            if (dist <= atkRange)
            {
                FaceTarget(playerPos, dt);
                if (_attackTimer <= 0f)
                {
                    EnterState(ZombieState.Attack);
                }
                return;
            }

            float chaseSpeed = (Archetype != null ? Archetype.speed : 2.5f) * _speedMultiplier;
            if (archetypeId == "runner" && dist < 7.5f)
            {
                chaseSpeed *= 1.18f; // L3 Runner sprint surge
            }

            MoveTowards(playerPos, chaseSpeed, dt);

            if (_stateTimer <= 0f && dist > 26f)
            {
                EnterState(ZombieState.Investigate);
            }
        }

        private void TickAttack(float dt)
        {
            if (PlayerController.Instance != null)
            {
                FaceTarget(PlayerController.Instance.Position, dt);
            }

            if (!_attackHitDelivered && _stateTimer <= 0.52f)
            {
                _attackHitDelivered = true;
                DeliverAttackPayload();
            }

            if (_stateTimer <= 0f)
            {
                _attackTimer = Archetype != null ? Archetype.attackCooldown : 1.25f;
                EnterState(ZombieState.Chase);
            }
        }

        private void TickStagger()
        {
            if (_stateTimer <= 0f)
            {
                EnterState(ZombieState.Chase);
            }
        }

        private void DeliverAttackPayload()
        {
            if (PlayerController.Instance == null || PlayerController.Instance.Stats == null) return;

            Vector3 playerPos = PlayerController.Instance.Position;
            float dist = HorizontalDistance(transform.position, playerPos);
            float dmg = Archetype != null ? Archetype.damage : 14f;

            // L3 Spitter ranged acid attack
            if (archetypeId == "spitter" && dist > 2.3f)
            {
                Vector3 mouth = transform.position + Vector3.up * 1.45f + transform.forward * 0.45f;
                Vector3 target = playerPos + Vector3.up * 1.0f;
                AcidProjectile.Spawn(mouth, target, dmg, gameObject);
                return;
            }

            // Melee claw / bite hit check
            float maxReach = (Archetype != null ? Archetype.attackRange : 1.75f) + 0.55f;
            if (dist <= maxReach && Mathf.Abs(playerPos.y - transform.position.y) < 2.2f)
            {
                DamageInfo info = new DamageInfo(
                    dmg,
                    DamageType.Melee,
                    gameObject,
                    playerPos + Vector3.up * 1.1f,
                    (playerPos - transform.position).normalized,
                    false);
                PlayerController.Instance.Stats.TakeDamage(info);

                // Bite/claw carries LZR-9 infection risk
                float infectionAdd = archetypeId == "goliath" ? 6.0f : 2.5f;
                PlayerController.Instance.Stats.AddInfection(infectionAdd);
                PlayArchetypeSound("scratch", 0.85f);
            }
        }

        private void Die(DamageInfo killingBlow)
        {
            if (IsDead) return;
            IsDead = true;
            EnterState(ZombieState.Dead);

            PlayArchetypeSound("death", 1.0f);

            Collider[] cols = GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i].enabled = false;
            }

            if (Agent != null && Agent.enabled)
            {
                Agent.enabled = false;
            }

            if (ZombieDirector.Instance != null)
            {
                ZombieDirector.Instance.NotifyZombieKilled(this);
            }

            GameEvents.RaiseZombieKilled(archetypeId, floor);

            // Chance to drop supply item
            if (Random.value < 0.35f)
            {
                string table = archetypeId == "armored" ? "military" : "residential";
                var drops = ItemDatabase.RollLoot(table, new System.Random( (int)(Time.time * 1000f) ^ GetInstanceID() ));
                if (drops != null && drops.Count > 0 && !string.IsNullOrEmpty(drops[0].id))
                {
                    WorldPickup.Spawn(drops[0].id, Mathf.Max(1, drops[0].count), transform.position + Vector3.up * 0.35f);
                }
            }

            Destroy(gameObject, 6.5f);
        }

        private void MoveTowards(Vector3 targetPos, float speed, float dt)
        {
            // Check for closed doors blocking path and open them
            TryOpenDoorAhead();

            if (Agent != null && Agent.enabled && Agent.isOnNavMesh)
            {
                Agent.isStopped = false;
                Agent.speed = speed;
                Agent.SetDestination(targetPos);
                return;
            }

            // Direct kinematic fallback if NavMesh surface is not active under agent
            Vector3 dir = targetPos - transform.position;
            dir.y = 0f;
            float mag = dir.magnitude;
            if (mag > 0.05f)
            {
                Vector3 stepDir = dir / mag;
                transform.position += stepDir * (speed * dt);
                Quaternion targetRot = Quaternion.LookRotation(stepDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, dt * 8f);
            }
        }

        private void TryOpenDoorAhead()
        {
            Vector3 origin = transform.position + Vector3.up * 1.0f;
            if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, 1.35f, ~0, QueryTriggerInteraction.Ignore))
            {
                Door door = hit.collider.GetComponentInParent<Door>();
                if (door != null && !door.locked && PlayerController.Instance != null)
                {
                    PlayAnim("DoorOpen");
                    door.Interact(PlayerController.Instance);
                }
            }
        }

        private void StopMovement()
        {
            if (Agent != null && Agent.enabled && Agent.isOnNavMesh)
            {
                Agent.isStopped = true;
                Agent.ResetPath();
            }
        }

        private void FaceTarget(Vector3 targetPos, float dt)
        {
            Vector3 dir = targetPos - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion rot = Quaternion.LookRotation(dir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, rot, dt * 10f);
            }
        }

        public void PlayAnim(string clipName)
        {
            if (Animator == null || string.IsNullOrEmpty(clipName)) return;
            if (_currentClip == clipName && clipName != "Attack" && clipName != "Attack2" && clipName != "Hit")
            {
                return;
            }
            _currentClip = clipName;
            Animator.CrossFadeInFixedTime(clipName, 0.15f);
        }

        private void PlayArchetypeSound(string action, float volume)
        {
            string baseId = "zmb_" + archetypeId + "_" + action;
            AudioManager.Play(baseId, transform.position, volume);
        }

        private static float HorizontalDistance(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x;
            float dz = a.z - b.z;
            return Mathf.Sqrt(dx * dx + dz * dz);
        }
    }
}
