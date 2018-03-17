namespace U3DExtends
{
    //���ܺͿ�ݼ�������
    public static class Configure
    {
        //�Ƿ��������е��Ҽ��˵�
        public static bool IsShowSceneMenu = true;

        //ѡ��ͼƬ�ڵ���ѡͼƬʱ������ڵ㸳�ϸ�ͼ
        public static bool IsEnableFastSelectImage = true;

        //��UI prefab����ͼƬ��scene����ʱ�����ҵ�����µ�Canvas���������ϣ��������û�л����ʹ���һ��
        public static bool IsEnableDragUIToScene = true;

        //�Ƿ����ü�ͷ�����ƶ�UI�ڵ�
        public static bool IsMoveNodeByArrowKey = true;

        //�������ʱ�Ƿ���Ҫ��ʾ����ɹ�����ʾ��
        public static bool IsShowDialogWhenSaveLayout = true;

        //������Ϸʱ�����ص�UITestNodeName�ڵ�
        public static bool HideAllUIWhenRunGame = true;
        
        //һ��Ӳο�ͼ�ʹ�ѡ��ͼƬ��
        public static bool OpenSelectPicDialogWhenAddDecorate = true;

        //��·������Ϊ�գ����ú��״ε��뱾���ʱ�ͻ���ظ�Ŀ¼�µ�����prefab
        //public const string PrefabWinFirstSearchPath = "Assets/LuaFramework/AssetBundleRes/ui/uiComponent/prefab";
        public const string PrefabWinFirstSearchPath = "";

        //��ݼ�����  �˵����ݼ���%#&1 ����ľ��ǣ�Ctrl + Shift + Alt + 1
        public static class ShortCut
        { 
            //����ѡ�нڵ�ȫ�����ַ�����ϵͳ���а�
            public const string CopyNodesName = "%#c";

            //������ʾ��εĿ�ݼ�
            public const string MoveNodeUp = "%UP";
            public const string MoveNodeTop = "%#UP";
            public const string MoveNodeDown = "%DOWN";
            public const string MoveNodeBottom = "%#DOWN";

            //������������н���
            public const string SortAllCanvas = "";
            //ɾ��UITestNodeName�ڵ��µ����н���
            public const string ClearAllCanvas = "";

            //���ؽ���
            public const string LoadUIPrefab = "%#l";
            //�������
            public const string SaveUIPrefab = "%#s";//Cat!TODO:�κ�s��ؿ�ݼ�����Ч������ֻ����SceneView.onSceneGUIDelegate�ﴦ����
        }

        //���б༭�����Canvas���ŵ��˽ڵ��ϣ��ɶ��ƽڵ���
        public static string UITestNodeName = "UITestNode";
        public const string ResPath = "UIEditor/Res/";
        public const string ResAssetsPath = "Assets/" + ResPath;
    }
}
