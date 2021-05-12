import { FontSizes, Label, mergeStyleSets } from '@fluentui/react';
import React from 'react';

export default function PageTitle(props: any) {

    const classNames = mergeStyleSets({
        pageTitle: {
            fontSize: FontSizes.large,
            padding: '10px 5px 5px 10px'            
        }
    });
    
    return (
        <div>
            <Label className={classNames.pageTitle}> {props.title} </Label>
                <br />
        </div>
    );
}
