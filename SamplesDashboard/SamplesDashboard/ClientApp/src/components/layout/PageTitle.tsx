import { FontSizes, Label, mergeStyleSets } from 'office-ui-fabric-react';
import React from 'react';

export default function PageTitle(props: any) {

    const classNames = mergeStyleSets({
        pageTitle: {
            fontSize: FontSizes.large,
            padding: '5px 5px 0px 10px'            
        }
    });
    
    return (
        <div>
            <Label className={classNames.pageTitle}> {props.title} </Label>
                <br />
        </div>
    );
}
