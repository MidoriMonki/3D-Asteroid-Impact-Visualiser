using UnityEngine;

<<<<<<< HEAD
public class LinkedList{
=======
public class LinkedList
{
>>>>>>> frontend
    private LinkedList head;
    //private float distanceH;
    private LinkedList tail;

    //private float distanceT;
    private Vector3 position;

<<<<<<< HEAD
    public LinkedList(LinkedList head, LinkedList tail, Vector3 position){
=======
    public LinkedList(LinkedList head, LinkedList tail, Vector3 position)
    {
>>>>>>> frontend
        this.head = head;
        this.tail = tail;
        this.position = position;
    }

<<<<<<< HEAD
    public LinkedList(Vector3 position){
=======
    public LinkedList(Vector3 position)
    {
>>>>>>> frontend
        head = null;
        tail = null;
        this.position = position;
    }

<<<<<<< HEAD
    public LinkedList getHead(){
        return head;
    }
    public LinkedList getTail(){
        return tail;
    }
    public Vector3 getPosition(){
        return position;
    }

    public void setHead(LinkedList head){
        this.head = head;
    }
    public void setTail(LinkedList tail){
        this.tail = tail;
    }
    public void setPosition(Vector3 position){
        this.position = position;
    }

    public LinkedList getRoot(){
        if (head == null){
            return this;
        }else{
            return head.getRoot();
        }
    }
    
/*
    public LinkedList insert(Vector3 position){
        if(distanceT == 0 || Vector3.Distance(position, this.position) < distanceT){
            LinkedList temp = new LinkedList(this, this.tail, position);
            this.tail.setHead(temp);
            this.tail = temp;
        }
        else if(distanceT == 0 || Vector3.Distance(position, this.position) < distanceT){
            LinkedList temp = new LinkedList(this, this.tail, position);
            this.tail.setHead(temp);
            this.tail = temp;
        }
    }
*/

}
=======
    public LinkedList getHead()
    {
        return head;
    }
    public LinkedList getTail()
    {
        return tail;
    }
    public Vector3 getPosition()
    {
        return position;
    }

    public void setHead(LinkedList head)
    {
        this.head = head;
    }
    public void setTail(LinkedList tail)
    {
        this.tail = tail;
    }
    public void setPosition(Vector3 position)
    {
        this.position = position;
    }

    public LinkedList getRoot()
    {
        if (head == null)
        {
            return this;
        }
        else
        {
            return head.getRoot();
        }
    }

    /*
        public LinkedList insert(Vector3 position){
            if(distanceT == 0 || Vector3.Distance(position, this.position) < distanceT){
                LinkedList temp = new LinkedList(this, this.tail, position);
                this.tail.setHead(temp);
                this.tail = temp;
            }
            else if(distanceT == 0 || Vector3.Distance(position, this.position) < distanceT){
                LinkedList temp = new LinkedList(this, this.tail, position);
                this.tail.setHead(temp);
                this.tail = temp;
            }
        }
    */

}
>>>>>>> frontend
