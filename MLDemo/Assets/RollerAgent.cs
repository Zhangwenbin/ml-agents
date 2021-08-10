using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace EG
{
    //激活虚拟环境
    // 创建虚拟环境所在的文件夹 md python-envs
    // 创建一个名为sample-envexecute 的新环境python -m venv python-envs\sample-env
    // 激活环境执行 python-envs\sample-env\Scripts\activate
    // 使用升级到最新的 pip 版本 pip install --upgrade pip

    //开始训练
    // 导航到您克隆ml-agents存储库的文件夹。
    // 运行mlagents-learn config/ppo/3DBall.yaml --run-id=first3DBallRun。

    // 2.观察进度
    // 打开命令或终端窗口。
    // 导航到您克隆ml-agents存储库的文件夹。
    // tensorboard --logdir results
    public class RollerAgent : Agent
    {
        private Rigidbody _rigidbody;

        public Transform target;
        // Start is called before the first frame update
        void Start()
        {

            _rigidbody = GetComponent<Rigidbody>();
        }

        public override void OnEpisodeBegin()
        {
            // If the Agent fell, zero its momentum
            if (this.transform.localPosition.y < 0)
            {
                this._rigidbody.angularVelocity = Vector3.zero;
                this._rigidbody.velocity = Vector3.zero;
                this.transform.localPosition = new Vector3( 0, 0.5f, 0);
            }

            // Move the target to a new spot
            target.localPosition = new Vector3(Random.value * 8 - 4,
                0.5f,
                Random.value * 8 - 4);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            // Target and Agent positions
            sensor.AddObservation(target.localPosition);
            sensor.AddObservation(this.transform.localPosition);

            // Agent velocity
            sensor.AddObservation(_rigidbody.velocity.x);
            sensor.AddObservation(_rigidbody.velocity.z);
        }

        public float forceMultiplier=10;
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            // Actions, size = 2
            Vector3 controlSignal = Vector3.zero;
            controlSignal.x = actionBuffers.ContinuousActions[0];
            controlSignal.z = actionBuffers.ContinuousActions[1];
            _rigidbody.AddForce(controlSignal * forceMultiplier);

            // Rewards
            float distanceToTarget = Vector3.Distance(this.transform.localPosition, target.localPosition);

            // Reached target
            if (distanceToTarget < 1.42f)
            {
                SetReward(1.0f);
                EndEpisode();
            }

            // Fell off platform
            else if (this.transform.localPosition.y < 0)
            {
                EndEpisode();
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var continuousActionsOut = actionsOut.ContinuousActions;
            continuousActionsOut[0] = Input.GetAxis("Horizontal");
            continuousActionsOut[1] = Input.GetAxis("Vertical");
        }
    }
}
