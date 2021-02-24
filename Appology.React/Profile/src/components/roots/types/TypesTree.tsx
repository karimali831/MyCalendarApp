import * as React from 'react';
import { IGroup } from 'src/models/IGroup';
import { IUserType } from 'src/models/IUserType';
import Tree from 'antd/lib/tree';
import 'antd/dist/antd.css'
import { FaPlus, FaUserFriends } from 'react-icons/fa';

const { DirectoryTree } = Tree;

export interface IOwnState {
    userTypes: IUserType[],
}

export interface IOwnProps {
    userTypes: IUserType[],
    group: IGroup,
    selectedTypeId?: number,
    selectedType: (type?: IUserType, addToKey?: number) => void,
    moveType: (id: number, dropPos: number, moveToNodE?: IUserType) => void
}


export class TypesTree extends React.Component<IOwnProps, IOwnState> {

    constructor(props: IOwnProps) {
        super(props);

        this.state = {
            userTypes: this.props.userTypes
        };
    }


    public componentDidUpdate = (prevProps: IOwnProps, prevState: IOwnState) => {
        if (JSON.stringify(this.props.userTypes) !== JSON.stringify(prevProps.userTypes)) {
            this.setState({ userTypes: this.props.userTypes })
        }
    }


    public render() {
        return(
            <DirectoryTree
                multiple={true}
                draggable={this.props.group.nodes > 0}
                defaultExpandAll={false}
                onSelect={(keys: any, info: any) => this.props.selectedType(info.node)}
                onDrop={this.onDrop}
                titleRender={(node: any) => this.titleRender(node)}
                treeData={this.state.userTypes}
            />
        )
    }

    private titleRender = (node: IUserType) : React.ReactNode => {
        // console.log(node)
        return <>
            {node.title} 
            <span className="float-right">
                {node.inviteeIdsList.length > 0 && <FaUserFriends style={{ marginRight: 5 }} />}
                {
                    this.props.group.nodes > 0 && 
                    <FaPlus onClick={(e) => this.addToParentClick(e, Number(node.key))} />
                }
            </span>
        </>;
    }

    private addToParentClick = (e: React.MouseEvent<SVGElement, MouseEvent>, addToKey: number) => {
        e.stopPropagation();
        this.props.selectedType(undefined, addToKey)
    }

    private onDrop = (info: any) => {
        // console.log(info)
        this.props.moveType(info.dragNode.key, info.dropPosition, info.node, );
    }



    // private getNodeFromTree = (nodes: IUserType[], nodeId: number): IUserType | null => {

    //     let tree: IUserType | null = null;

    //     nodes.map(node => {
    //         if (node.key === nodeId) {
    //             return node;
    //         } 
    //         else if (node.children != null && node.children.length > 0) {
    //             this.getNodeFromTree(node.children, nodeId);
    //         }
    //     })
    //     return tree;
    // }

    // private insertNodeIntoTree = (nodes: IUserType[], nodeId: number, newNode: IUserType) => {

    //     const tree: IUserType[] = [];

    //     nodes.map(node => {
    //         if (node.key === nodeId) {
    //             // get new id
    //             const n = 999;
            
    //             if (newNode) {
    //                 newNode.key = n;
    //                 // newNode.children = [];
    //                 // node.children.push(newNode);
    //             }
        
    //         } else if (node.children != null) {
    //             this.insertNodeIntoTree(node.children, nodeId, newNode);
    //         }

    //         tree.push(node)
    //     })
    
    //     return tree;
    // }

    

    // private moveNodeInTree = (nodes: IUserType[], nodeId: number, moveToNode?: number) : IUserType[] => {
    //     const tree: IUserType[] = [];

    //     nodes.map(node => {
    //         if (node.key === nodeId) {
    //             // node.title = newNode.title;
    //             node.superTypeId = moveToNode;
    //         } else if (node.children != null) {
    //             this.moveNodeInTree(node.children, nodeId, moveToNode);
    //         }

    //         tree.push(node)
    //     })

    //     return tree;
    // }

    // private deleteNodeFromTree = (nodes: IUserType[], nodeId: number) => {

    //     const tree: IUserType[] = [];

    //     nodes.map(node => {
    //         if (node.children != null) {
    //             const filtered = node.children.filter(f => f.key === nodeId);

    //             if (filtered && filtered.length > 0) {
    //                 node.children = node.children.filter(f => f.key !== nodeId);
    //                 return;
    //             }

    //             this.deleteNodeFromTree(node.children, nodeId,);
    //         }
    //         tree.push(node);
    //     })

    //     return tree;
    
    // }
}