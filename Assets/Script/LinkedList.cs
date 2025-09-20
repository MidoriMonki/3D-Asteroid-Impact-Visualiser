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


/*
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
*/ 

/*
    public LinkedList reverseListSegment(bool check){
        if(tail != null && getPosition().x == 0){
            LinkedList store = head;
            setHead(getTail());
            setTail(store);
            if(check){
                setPosition(new Vector3(10, getPosition().y, getPosition().z));
            }
            return getHead().reverseListSegment(false);
        }else{
            LinkedList store = head;
            setHead(getTail());
            setTail(store);
            setPosition(new Vector3(0, getPosition().y, getPosition().z));
            return this;
        }
    }
    */

/*
    public void reverseList(LinkedList node){
        //Important disclaimer, this function works by checking if each part of the linked list needs
        //to be separated, evaluates by checking the y of each section's head/tail, separate each section by x!=0 (or x==10f)

        LinkedList returnNode = null;
        
        //While there are still segments left, check if we should reverse
        while(node.getTail()!=null){
            //check if we should reverse
            if(node.getPosition().y <= node.getTail().getPosition().y){
                node.reverseListSegment(true);
                if(returnNode == null){
                    returnNode = new LinkedList(null, node.getTail(), node.getPosition())
                }else{
                    //Make second element point to the tail of final node
                    node.getTail().setHead(returnNode.getEnd().getTail());
                    returnNode.getEnd().setTail(new LinkedList(returnNode.getEnd(), node.getTail(), node.getPosition()));

                }

                node = node.getTail();
            }


        }
    }
    */



    public LinkedList getHead(){
        return head;
    }
    public LinkedList getTail(){
        return tail;
    }
    public Vector3 getPosition(){
        return node.pos;
    }
    public float?[] getParameter()
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
        if (tail == null){
            return this;
        }else{
            return tail.getEnd();
        }
    }

/*
    public LinkedList getSegmentRoot(){
        if (head == null || head != null && getHead().getPosition().x != 0){
            return this;
        }else{
            return head.getSegmentRoot();
        }
    }
    */

    public LinkedList getSegmentEnd()
    {
        if (tail == null || getPosition().x != 0){
            return this;
        }else{
            return tail.getSegmentEnd();
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

