# AlpoLib Res

## Generic Prefab
- Mono 상속된 클래스 하나와 addressable, resouces 에 포함된 하나 (또는 다수)의 prefab과 연결하는 방식입니다.
- 클래스 속성으로 PrefabPathAttribute 를 선언합니다.
```cs
[PrefabPath("ADDR_PATH")]
public class MyPrefab : MonoBehaviour
{
}
```
- path 에 프리팹의 경로를 기입합니다.
- source 에 addressable, resources 여부를 기입합니다. (기본값은 addressable)
- addressable 일 경우, path 에는 해당 프리팹의 인스펙터 상단에 기재된 addressable 주소를 그대로 기입합니다.
- 해당 스크립트를 프리팹 루트에 add component 시켜서 저장합니다.
- 다음과 같이 해당 프리팹을, 제네릭 방식으로 Instantiate 할 수 있습니다.
```cs
MyPrefab instance = GenericPrefab.InstantiatePrefab<MyPrefab>();
```
- 동기, 비동기 방식 모두 있으며, Instantiate 전의 Load만 진행할 수도 있습니다.
