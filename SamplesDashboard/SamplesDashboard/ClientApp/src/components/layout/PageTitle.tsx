import { FontSizes, Label, mergeStyleSets } from 'office-ui-fabric-react';
import React from 'react';

export default function PageTitle(props: any) {

    const classNames = mergeStyleSets({
        pageTitle: {
            fontSize: FontSizes.large
        }
    });
    
    return (
        <div>
            <Label className={classNames.pageTitle}> {props.title} </Label>
                <br />
        </div>
    );
}
