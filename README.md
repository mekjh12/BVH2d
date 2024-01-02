[Reference] Erin Catto, Dynamic Bounding Volume Hierarchies, Blizzard Entertainment

아래는 2d실행화면이다. 
![image](https://github.com/mekjh12/BVH2d/assets/122244587/04564aeb-2c5f-496b-8174-bd9feeb0d075)

21번 노드가 이동 중 EnhanceBox와 충돌하여 노드를 제거하고 새롭게 노드를 추가해 준다.
![image](https://github.com/mekjh12/BVH2d/assets/122244587/9cc26fab-17ab-4090-9620-226fc2313557)


# Branch and Bound 알고리즘

![image](https://github.com/mekjh12/BVH2d/assets/122244587/4d1ff281-583b-4716-9493-cc661c301e1d)

$C_7$의 비용은 다음과 같다.

$$ C_{7}=S(7\cup L)+\Delta S_{3}+\Delta S_{1} $$


아래 그림과 같이 노드 8 또는 9의 위치에 따라 $C_8, C_9$가 변하므로 
![image](https://github.com/mekjh12/BVH2d/assets/122244587/344e3912-6b57-4e39-8778-865ea996bb24)

<b>lower bound cost for nodes 8 and 9</b>

** 8과 9의 노드는 크기와 위치를 확정할 수 없으므로 L이 7에 삽입하는 비용(cost)의 하계값(lower bound)는 아래와 같다.
$$C_{low}=S(L)+\Delta S_{7}+\Delta S_{3}+\Delta S_{1}$$

이를 종합하면, $C_{best}\leq C_{low}$이면 노드 7의 하위 트리의 노드에 삽입하여도 그 비용은 $C_{best}$보다 클 수 밖에 없다.
따라서 $C_{best}\leq C_{low}$이면 노드 7의 하위 노드를 탐색할 필요가 없다. 

반대로 $C_{low} < C_{best}$ 이면 노드7의 하위 노드를 탐색해야 하므로 큐에 노드 7의 자식 노드 8과 9를 enqueue한다.

# Sorted Insert Problem and RotateNode

![image](https://github.com/mekjh12/BVH2d/assets/122244587/5642ef1b-e00d-4d0e-993f-5dad449f2d83)

Tree rotations : Re-arranging a tree to reduce the SAH cost
Tree rotations are used for AVL trees to keep them balanced. They can also be used for bounding volume hierarchies to reduce the surface area and to mitigate the problems introduced by sorted input.



