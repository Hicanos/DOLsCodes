# DOLsCodes
Dice Of Labyrinth에서 사용한 코드 샘플입니다.

## 캐릭터
### Editor: SO Generator-json으로 파싱한 데이터를 SO 형태로 변환
= 왜 변환했는가? (Json을 파싱한 Data.cs가 따로 있음에도)
: Scriptable Object로 작성할 경우, 인스펙터 창에서 각 요소의 정보를 직관적으로 확인할 수 있다.

### Scripts
- Character.cs: Character의 기본 정보(Lobby, Battle 공통)
- LobbyCharacter.cs: 메인(로비)의 캐릭터창에서 확인할 수 있는 데이터. EXP 포션 소비를 통해 레벨업을 하고, 그에 따른 능력치 변화가 발생함
- BattleCharacter.cs: LobbyCharacter의 데이터를 복사하여, 전투 시 사용되는 캐릭터 데이터(전투 중 실시간으로 데이터가 변화함: 아티팩트, 각인, 스킬 등의 요소로 인해)
- SpawnedCharacter.cs: 전투 시 캐릭터 프리펩을 조종하는 스크립트(애니메이션, 효과음 등의 조절)
- GetCharacter.cs: 캐릭터를 획득하는 스크립트(영웅 획득 로직, 즉, 가챠)
- CharData.cs: CharData.json 정보를 로드한 데이터. (처음 SO 생성 후에는 기능이 없음)


## Data관리
### DataManagers: 각 분야별 데이터 관리자
- ItemManager.cs   
- BuffManager.cs   
- CharacterManager.cs   
- StaticDataManager.cs: 정적 데이터 저장소. Stage에서 저장할 SO를 찾아내는 용도   

```
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임에서 사용되는 정적데이터(아티팩트, 각인, 적 등)의 데이터 매니저
/// Artifact, Engraving, Enemy 등의 SO를 들고다니며 이름 기반으로 일관 관리
/// </summary>
public class StaticDataManager : MonoBehaviour
{
    // 싱글톤 인스턴스. 게임 내에서 정적 데이터에 접근할 때 사용
    public static StaticDataManager Instance { get; private set; }

    // 인스펙터에서 할당하는 아티팩트 SO 리스트
    [SerializeField] private List<ArtifactData> artifacts;
    // 인스펙터에서 할당하는 각인 SO 리스트
    [SerializeField] private List<EngravingData> engravings;
    // 인스펙터에서 할당하는 적 SO 리스트
    [SerializeField] private List<EnemyData> enemies;
    // 랜덤 이벤트 SO 리스트 (인스펙터에서 할당)
    [SerializeField] private List<RandomEventData> randomEvents;

    // 이름(string)으로 SO를 빠르게 찾기 위한 딕셔너리
    private Dictionary<string, ArtifactData> artifactDict = new Dictionary<string, ArtifactData>();
    private Dictionary<string, EngravingData> engravingDict = new Dictionary<string, EngravingData>();
    private Dictionary<string, EnemyData> enemyDict = new Dictionary<string, EnemyData>();
    private Dictionary<string, RandomEventData> randomEventDict = new Dictionary<string, RandomEventData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitDictionaries(); // SO 리스트를 이름 기반 딕셔너리에 등록
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// SO 리스트를 이름(string) 기반 딕셔너리에 등록하여 빠른 조회 가능하게 함
    /// </summary>
    private void InitDictionaries()
    {
        artifactDict.Clear();
        foreach (var so in artifacts)
            if (so != null && !string.IsNullOrEmpty(so.ArtifactName))
                artifactDict[so.ArtifactName] = so;

        engravingDict.Clear();
        foreach (var so in engravings)
            if (so != null && !string.IsNullOrEmpty(so.EngravingName))
                engravingDict[so.EngravingName] = so;

        enemyDict.Clear();
        foreach (var so in enemies)
            if (so != null && !string.IsNullOrEmpty(so.EnemyName))
                enemyDict[so.EnemyName] = so;

        randomEventDict.Clear();
        if (randomEvents != null)
        {
            foreach (var so in randomEvents)
                if (so != null && !string.IsNullOrEmpty(so.EventName))
                    randomEventDict[so.EventName] = so;
        }
    }

    /// <summary>
    /// 이름(string)으로 아티팩트 SO를 반환
    /// </summary>
    public ArtifactData GetArtifact(string name) =>
        artifactDict.TryGetValue(name, out var so) ? so : null;
    /// <summary>
    /// 이름(string)으로 각인 SO를 반환
    /// </summary>
    public EngravingData GetEngraving(string name) =>
        engravingDict.TryGetValue(name, out var so) ? so : null;
    /// <summary>
    /// 이름(string)으로 적 SO를 반환
    /// </summary>
    public EnemyData GetEnemy(string name) =>
        enemyDict.TryGetValue(name, out var so) ? so : null;
    /// <summary>
    /// 이름(string)으로 랜덤 이벤트 SO를 반환
    /// </summary>
    public RandomEventData GetRandomEvent(string name) =>
        randomEventDict.TryGetValue(name, out var so) ? so : null;

    /// <summary>
    /// 전체 아티팩트 SO 리스트 반환
    /// </summary>
    public List<ArtifactData> GetAllArtifacts() => artifacts;
    /// <summary>
    /// 전체 각인 SO 리스트 반환
    /// </summary>
    public List<EngravingData> GetAllEngravings() => engravings;
    /// <summary>
    /// 전체 적 SO 리스트 반환
    /// </summary>
    public List<EnemyData> GetAllEnemies() => enemies;
}

  ```
## Items
- 각 아이템의 종류에 따른 Scriptable Object   
- 현재 구현 요소: 돌파석, EXP 포션, 스킬북   
- ItemSO와 아이템 타입 Enum을 이용하여 소비아이템 외에도 제작 아이템 등으로 확장 가능   

## DataSaver & GameManager
- 데이터 저장 및 로드를 담당하는 **DataSaver**
- 게임 시작과 종료 시 데이터 로드 및 복원, 저장을 담당하는 **GameManager**

  ### GameManager
  - Coroutine을 통해 각 Instance가 생성된 후 DataSaver의 Load로직을 실행시키고 있다.
    (대기 시키지 않으면, Load가 Instance보다 선행될 수 있음)
