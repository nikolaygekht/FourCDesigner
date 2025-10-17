# Method

Training from the BACK of the Room! (TBR) is a brain science-based approach that makes learning engaging and effective. Created by Sharon L. Bowman, educator, trainer, and author of the bestselling books Training from the BACK of the Room! and Using Brain Science to Make Training Stick, TBR turns passive listeners into active participants.

# Common Definitions

## Learning outcomes

Learning outcomes are specific statements of what students will be able to do when they successfully complete a learning experience (whether it's a project, course or program). They are always written in a student-centered, measurable fashion that is concise, meaningful, and achievable.

## 4C Approach

Every lesson in TBR method is build around 4C template. 4C represents four phases of the lesson: connections, concepts, concrete practice and conclusions.

### Connections

Learners make connections with what they already know about the topic, with what they want to learn. 

Learners also make connection with each other. 

### Concepts

Learners take in information in multi-sensory way. 

Multi-sensory way means engaging all four learning styles: visual (learning by seeing), aural (learning by listening), reading and kinesthetic (learning by application).

### Concrete Practice

Learners practice a skill or repeat a procedure being learned, review the content. 

### Conclusions

Learners summarize what they have learned, evaluate it, makes action plans to use it and celebrate the learning. 

### Six Trumps

In a nutshell, here are the six brain science principles that make training stick. When it comes to learning:
* Movement trumps sitting.
* Talking trumps listening.
* Images trump words.
* Writing trumps reading.
* Shorter trumps longer.
* Different trumps same.

# Domain model

## User 

The user of the application. 

```ts
{
    username : string,
    firstName : string,
    lastName : string,
    passwordHash : string,
    email : string,
    communicationsAllowed : boolean
    activeUser : boolean
}
```

The user has the following properties:

* username : string of 32 alphanumeric characters
* firstName : string of 32 alphanumeric characters 
* lastName : string of 32 alphanumeric characters (optional)
* password : string (long enough to store SHA256 hash)
* email : string of 256 characters
* communicationsAllowed : The flag regulating whether the user wants to receive marketing emails : boolean
* activeUser : The flag indicating that the user is active

NOTE: The password must not be stored in the system. Store SHA256 hash instead. 

## Lesson

The lesson that user is currently creating. 

NOTE: The lesson will never be stored permanently on the server side, howerver it is stored on the client (in the browser storage) to avoid losing the data in case of 
connection interruptions or time outs. 

NOTE: All properties for which the type is not exactly specified are rich text properties (at least having bold, italic and bullet list options)

NOTE: It the PoC so, the system works in english language only

NOTE: It the PoC so, the system doesn't store or track history of the lessons. 

The lesson has the following properties. 

```ts
{
    topic : string,
    audience : string,
    learningOutcomes : string,
    connections : {
        timing : int,
        goal : string,
        plan : {
            activities : string,
            materialsToPrepare : string
        }
    },
    concepts : {
        timing : int,
        needToKnow : string,
        goodToKnow : string,
        plan : {
            theses : string,
            structure : string,
            activities : string,
            materialsToPrepare : string
        }
    }, 
    concretePractice : {
        timing : int,
        desiredOutput : string,
        focusArea : string,
        plan : {
            activities : string,
            details : string,
            materialsToPrepare : string
        }
    },
    conclusions : {
        timing : int,
        goal : string,
        plan : {
            activities : string,
            materialsToPrepare : string
        }
    }  
}
```

## topic 

A short description of the desired purpose of the lesson

## audience 

The description of who are the audience (learners) and what are their characteristics (e.g. age, education level, job functions, language, etc) and what are their needs (e.g. what motivates them to attend the lesson).

## learningOutcomes 

The description of the learners need can do after attending the lesson. 

## connections

The description of the connections phase (compound property)

### timing

The `integer` property that contains the number of minutes allocated for the phase. 
 
### goal

The goal of the connections phase. 

### plan

The plan of connections phase (compound property)

#### activities

 The activity or the activities to perform during the phase.
 
#### materialsToPrepare

 The list of the materials (if any) the teacher need to prepare for the phase. 

## concepts 

The description of the concepts phase (compound property)

### timing

The `integer` property that contains the number of minutes allocated for the phase. 
 
### needToKnow

The list of the "need to know" concepts.

### goodToKnow

The list of the "good to know" concepts.

### plan

The plan of the concepts phase (compound property)

#### theses

The list of the theses to be delivered

#### structure

The structure of the delivery part. 

#### activities

List of activities to engage all kind of learners (VARK) and all six trumps 

#### materialsToPrepare

The list of the materials to prepare. 

## concretePractice

The description of the concrete practice phase (compound property)

### timing

 The `integer` property that contains the number of minutes allocated for the phase. 
 
### desiredOutput

What is the desired output of the exercise

### focusArea

The area of the focus during the exercise (where we expect the most hesitations)

### plan

The plan of the concrete practice phase (compound property)

#### activities

The list of the activities to perform during the exercice.

#### details

The plan on how we perform these activities.

#### materialsToPrepare

What kind of materials needs to be prepared. 

## Conclusions

The description of the conclusions phase (compound property)

### Timing

The `integer` property that contains the number of minutes allocated for the phase. 
 
### Goal

The goal of the conclusions phase. 
 
### plan

#### activities

The activity or the activities to perform during the phase.
 
#### materialsToPrepare

The list of the materials (if any) the teacher need to prepare for the phase. 


