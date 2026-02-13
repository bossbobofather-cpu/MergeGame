using Mirror;
using MyProject.Common.Bootstrap;
using UnityEngine;

namespace MyProject.MergeGame.Unity.Network
{
    public class MergeGameNetworkManager : NetworkManager, IManager
    {
        [SerializeField] MergeGameServerAdapter _serverAdapterPrefab;
        MergeGameServerAdapter _serverAdapterInstance;
        /// <summary>
        /// Initialize 함수를 처리합니다.
        /// </summary>

        public void Initialize()
        {
            // 핵심 로직을 처리합니다.
            User.InitializeFromRuntime();
        }

        //?쒕쾭留?吏꾩엯 ???몄텧 ?쒕떎.
        /// <summary>
        /// OnStartServer 함수를 처리합니다.
        /// </summary>
        public override void OnStartServer()
        {
            // 핵심 로직을 처리합니다.
            base.OnStartServer();

            _serverAdapterInstance = GameObject.Instantiate(_serverAdapterPrefab);
            Debug.Assert(_serverAdapterInstance != null, "Failed Instantiate MergeGameServerAdapter");
            if(_serverAdapterInstance) _serverAdapterInstance.Initialize();
        }

        //?쒕쾭媛 以묒??????몄텧 ?⑸땲??
        /// <summary>
        /// OnStopServer 함수를 처리합니다.
        /// </summary>
        public override void OnStopServer()
        {
            // 핵심 로직을 처리합니다.
            if (_serverAdapterInstance != null)
            {
                // ?대뙌???몄뒪?댁뒪瑜??뚭눼?섏뿬 由ъ냼???꾩닔瑜?諛⑹??⑸땲??
                // ?대뙌?곗쓽 OnDestroy?먯꽌 ?대깽??援щ룆 ?댁젣 ?깆씠 泥섎━?⑸땲??
                Destroy(_serverAdapterInstance.gameObject);
                _serverAdapterInstance = null;
            }
            
            base.OnStopServer();
        }

        /// <summary>
        /// ?대씪?댁뼵?멸? ?뚮젅?댁뼱 異붽?瑜??붿껌?????몄텧?⑸땲??
        /// ?꾩옱 ?꾪궎?띿쿂?먯꽌??Mirror???먮룞 ?뚮젅?댁뼱 ?앹꽦???ъ슜?섏? ?딆쑝誘濡?        /// 湲곕낯 ?숈옉??留됯린 ?꾪빐 蹂?硫붿냼?쒕? 鍮꾩썙?〓땲??
        /// </summary>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // base.OnServerAddPlayer(conn); // 湲곕낯 ?꾨━???앹꽦 濡쒖쭅? ?몄텧?섏? ?딆쓬
        }
    }
}
