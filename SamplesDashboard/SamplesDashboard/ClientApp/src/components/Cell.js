import * as React from 'react';

export default function Cell({
    content,
    header,
}) {

    const cellMarkup = header ? (
        <th className="Cell Cell-header">
            {content}
        </th>
    ) : (
            <td className="Cell">
                {content}
            </td>
        );

    return (cellMarkup);
}