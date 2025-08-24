using UnityEngine;

public class LinkedList
{
    private LinkedList head;
    private LinkedList tail;
    private Coordinate node;

    public LinkedList(LinkedList head, LinkedList tail, Coordinate node)
    {
        this.head = head;
        this.tail = tail;
        this.node = node;
    }

    public LinkedList(Coordinate node)
    {
        head = null;
        tail = null;
        this.node = node;
    }

    public LinkedList reverseList()
    {
        if(tail != null){
            LinkedList store = head;
            setHead(getTail());
            setTail(store);
            return getHead().reverseList();
        }
        else
        {
            setTail(getHead());
            setHead(null);
            return this;
        }
        
    }

    public LinkedList getHead(){
        return head;
    }
    public LinkedList getTail(){
        return tail;
    }
    public Vector3 getPosition(){
        return node.pos;
    }
    public float getParameter()
    {
        return node.parameter;
    }

    public void setHead(LinkedList head){
        this.head = head;
    }
    public void setTail(LinkedList tail){
        this.tail = tail;
    }
    public void setNode(Coordinate node){
        this.node = node;
    }
    public void setPosition(Vector3 pos)
    {
        node.pos = pos;
    }

    public LinkedList getRoot(){
        if (head == null){
            return this;
        }else{
            return head.getRoot();
        }
    }

    public LinkedList getEnd()
    {
        if (tail == null)
        {
            return this;
        }
        else
        {
            return tail.getEnd();
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

