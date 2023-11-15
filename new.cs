using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


public class SoccerPlayerAgent : Agent
{
    public GameObject ball;
    public GameObject goal;
    public float kickForce = 10f;
    public float boundaryRadius = 20f;

    public override void OnEpisodeBegin()
    {
        // Reset the environment at the beginning of each episode
        ball.transform.position = new Vector3(0f, 0.5f, 0f);
        ball.GetComponent<Rigidbody>().velocity = Vector3.zero;
        ball.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        // Randomly place the agent within the circular bounds
        transform.position = GetRandomPositionInBounds();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observe the agent's position
        sensor.AddObservation(transform.position);

        // Observe the ball's position
        sensor.AddObservation(ball.transform.position);

        // Observe the goal's position
        sensor.AddObservation(goal.transform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Convert continuous actions to discrete actions
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        // Apply force to move the agent
        Vector3 moveForce = new Vector3(moveX, 0f, moveZ);
        GetComponent<Rigidbody>().AddForce(moveForce * 10f);

        // Calculate the distance from the center of the circle
        float distanceToCenter = Vector3.Distance(Vector3.zero, transform.position);

        // Penalize the agent if it moves outside the circular area
        if (distanceToCenter > boundaryRadius)
        {
            AddReward(-0.1f);
        }

        // Kick the ball if the agent is close to it
        if (Vector3.Distance(transform.position, ball.transform.position) < 1.5f)
        {
            Vector3 kickDirection = (goal.transform.position - ball.transform.position).normalized;

            // Kick the ball towards the goal
            ball.GetComponent<Rigidbody>().AddForce(kickDirection * kickForce, ForceMode.Impulse);

            // Reward the agent for scoring a goal
            AddReward(1.0f);
        }

        // Penalize the agent if it misses the goal
        if (Vector3.Distance(ball.transform.position, goal.transform.position) > 2.5f)
        {
            AddReward(-0.1f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Manual control for testing purposes
        actionsOut.ContinuousActions[0] = Input.GetAxis("Horizontal");
        actionsOut.ContinuousActions[1] = Input.GetAxis("Vertical");
    }

    private Vector3 GetRandomPositionInBounds()
    {
        // Get a random position within the circular bounds
        float angle = Random.Range(0f, 2f * Mathf.PI);
        float radius = Random.Range(0f, boundaryRadius);
        float x = radius * Mathf.Cos(angle);
        float z = radius * Mathf.Sin(angle);

        return new Vector3(x, 0.5f, z);
    }
}